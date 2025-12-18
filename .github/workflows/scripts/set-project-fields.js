module.exports = async function run({ core, github, process }) {
  if (!process.env.ITEM_ID || !process.env.PROJECT_ID) {
    console.log('[Diag] item_id/project_id not found.');
    return;
  }
  if (!process.env.FIELDS) {
    console.log('[Diag] FIELDS not found.');
    return;
  }

  const projectId = process.env.PROJECT_ID;
  const itemId = process.env.ITEM_ID;

  function safeParse(name, raw) {
    try {
      return JSON.parse(raw);
    } catch (e) {
      core.setFailed(`[ParseError] ${name}: ${e.message}`);
      return null;
    }
  }

  const fields = safeParse('FIELDS', process.env.FIELDS);
  // fieldOptions may be null if no single select fields exist or parsing fails
  const fieldOptions = process.env.FIELD_OPTIONS ? safeParse('FIELD_OPTIONS', process.env.FIELD_OPTIONS) : {};

  if (!fields) return;

  const fieldIdByName = new Map(Object.entries(fields).map(([k, v]) => [k.trim().toLowerCase(), v]));
  const optionIdByFieldName = new Map(
    Object.entries(fieldOptions || {}).map(([fname, opts]) => [
      fname.trim().toLowerCase(),
      new Map(Object.entries(opts || {}).map(([n, id]) => [n.trim().toLowerCase(), id])),
    ]),
  );

  function getFieldId(fieldName) {
    const id = fieldIdByName.get(fieldName.trim().toLowerCase());
    if (!id) console.log(`Field not found: ${fieldName}`);
    return id;
  }

  function getOptionId(fieldName, value) {
    const opts = optionIdByFieldName.get(fieldName.trim().toLowerCase());
    if (!opts) return null;
    return opts.get(value.trim().toLowerCase());
  }

  async function updateField(fieldId, valueObj) {
     // valueObj example: { singleSelectOptionId: "..." } or { date: "..." } or { number: 123 } or { text: "..." }
     const mutation = `
       mutation($projectId: ID!, $itemId: ID!, $fieldId: ID!, $value: ProjectV2FieldValue!) {
         updateProjectV2ItemFieldValue(input: {
           projectId: $projectId
           itemId: $itemId
           fieldId: $fieldId
           value: $value
         }) { projectV2Item { id } }
       }
     `;
     await github.graphql(mutation, { projectId, itemId, fieldId, value: valueObj });
  }

  async function setAny(fieldName, value) {
    if (!value) return;
    const valStr = String(value).trim();
    if (!valStr) return;

    const fieldId = getFieldId(fieldName);
    if (!fieldId) return;

    console.log(`Setting ${fieldName} = ${valStr}`);

    // Try Single Select first
    const opts = optionIdByFieldName.get(fieldName.trim().toLowerCase());
    if (opts) {
      const optId = getOptionId(fieldName, valStr);
      if (optId) {
        await updateField(fieldId, { singleSelectOptionId: optId });
        return;
      }
      console.log(`Option '${valStr}' not found in ${fieldName}. Available:`, Array.from(opts.keys()));
      // Fallback: don't try to set text/number if it's strictly a single select field, it will fail.
      return;
    }

    // Date check (simple YYYY-MM-DD)
    if (/^\d{4}-\d{2}-\d{2}$/.test(valStr)) {
        // Assume it is a Date field
        try {
            await updateField(fieldId, { date: valStr });
            return;
        } catch(e) { /* ignore, maybe it's text */ }
    }

    // Number check
    if (!isNaN(parseFloat(valStr))) {
        // Try as number
        try {
            await updateField(fieldId, { number: parseFloat(valStr) });
            return;
        } catch(e) { /* ignore */ }
    }

    // Fallback to text
    try {
        await updateField(fieldId, { text: valStr });
    } catch(e) {
        console.log(`Failed to set ${fieldName} as text: ${e.message}`);
    }
  }

  // --- Main Execution ---
  // Environment variables mapped from workflow
  await setAny('Priority', process.env.PRIORITY);
  await setAny('Target date', process.env.DUE_DATE); // Due Date -> Target date
  await setAny('Start date', process.env.START_DATE);

  console.log('Done.');
};
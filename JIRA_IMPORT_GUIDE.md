# Jira Bug Import Guide

## File: jira_bugs_import.csv

This CSV file contains bug reports for the EduShpere project that can be imported into Jira.

## Import Instructions

### 1. Prepare Jira Project
- Ensure your Jira project has the following fields configured:
  - Issue Type (Bug)
  - Priority
  - Status
  - Components
  - Labels
  - Environment
  - Affects Version
  - Fix Version

### 2. Import Steps

#### Option A: Using Jira UI
1. Go to your Jira project
2. Click on **"..."** (More actions) → **Import issues from CSV**
3. Select the `jira_bugs_import.csv` file
4. Map CSV columns to Jira fields:
   - Issue Type → Issue Type
   - Summary → Summary
   - Description → Description
   - Priority → Priority
   - Status → Status
   - Assignee → Assignee
   - Reporter → Reporter
   - Components → Components
   - Labels → Labels
   - Environment → Environment
   - Steps to Reproduce → Custom field or Description
   - Expected Result → Custom field or Description
   - Actual Result → Custom field or Description
   - Affects Version → Affects Version
   - Fix Version → Fix Version
5. Review the preview
6. Click **Begin Import**

#### Option B: Using Jira REST API
```bash
# Use Jira REST API to import issues
curl -X POST \
  'https://your-jira-instance.atlassian.net/rest/api/3/issue/bulk' \
  -H 'Authorization: Basic YOUR_AUTH_TOKEN' \
  -H 'Content-Type: application/json' \
  -d @jira_bugs_import.json
```

### 3. Field Mapping Notes

- **Issue Type**: All issues are set to "Bug"
- **Priority**: High, Medium, Low
- **Status**: Default to "To Do" (adjust after import)
- **Assignee**: Set to "Unassigned" (assign after import)
- **Reporter**: Set to "QA Team" (update to actual reporter)
- **Components**: Backend, Frontend, Database, etc.
- **Labels**: Performance, Security, API, Testing, etc.
- **Environment**: Development, Production, Staging

### 4. Custom Fields

If your Jira instance has custom fields for:
- **Steps to Reproduce**: Map to custom field or include in Description
- **Expected Result**: Map to custom field or include in Description
- **Actual Result**: Map to custom field or include in Description

### 5. Post-Import Tasks

After importing:
1. Review all imported bugs
2. Assign bugs to appropriate team members
3. Update status based on current workflow
4. Add any missing information
5. Link related issues if needed
6. Add attachments (screenshots, logs) if available

## CSV Format

The CSV file uses the following format:
- **Delimiter**: Comma (,)
- **Quote Character**: Double quotes (")
- **Encoding**: UTF-8
- **Header Row**: Yes (first row contains field names)

## Bug Categories

The bugs in this file are categorized by:
- **Performance Issues**: Thread saturation, slow queries, memory issues
- **API Issues**: Missing validation, rate limiting, error handling
- **Configuration Issues**: Connection pool, thread pool, server limits
- **Testing Issues**: Missing unit tests, integration tests
- **Security Issues**: Rate limiting, input validation

## Notes

- All dates in the CSV are in format: YYYY-MM-DD or DD/MM/YYYY
- Version numbers follow semantic versioning (e.g., 1.0.0, 1.1.0)
- Some fields may need adjustment based on your Jira configuration
- Custom fields may need to be created in Jira before import

## Troubleshooting

### Common Issues

1. **Field Mapping Errors**
   - Ensure all field names match your Jira field names exactly
   - Check if custom fields exist in your Jira project

2. **Import Fails**
   - Verify CSV encoding is UTF-8
   - Check for special characters that need escaping
   - Ensure required fields are not empty

3. **Data Not Appearing**
   - Check field permissions in Jira
   - Verify user has permission to create issues
   - Review import logs for errors

## Support

For issues with importing, contact your Jira administrator or refer to:
- [Jira CSV Import Documentation](https://support.atlassian.com/jira-service-management-cloud/docs/import-data-from-a-csv-file/)










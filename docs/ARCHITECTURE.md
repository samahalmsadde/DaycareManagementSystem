# Implemented architecture

Browser (HTML + CSS + JavaScript)
  -> HTTP requests to ASP.NET Core Minimal API (.NET 10)
  -> DataStore singleton
  -> App_Data/daycare.json (local file)

## Data flow
1. The browser submits a child using POST /api/children.
2. The API validates the required names.
3. The DataStore locks access, updates its in-memory object and saves JSON to disk.
4. The API responds with the created record.
5. The browser refreshes the child list and dashboard.

## Not implemented
- Cloud hosting, cloud-managed database, authentication, encryption, monitoring service, ETL, or automatic cloud deployment.
- Those require a real cloud account, provider choice and security setup.

Do not enter real children's records into this learning demo.

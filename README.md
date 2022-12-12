
# Azure Blob Upload (dotnet, Angular)
## About
A complete example to upload file in blob containers, list all files from a container and view a file using SAS token. This example inclued backend (API) and frontend:
1. API (.net 7)
2. Frontend (Angular 15.0.3)

Frontend build using Angular, bootstrap. We have also integrate Ngx-bootstrap but not used much as this is just one page example, from where we are uploading a file and then showing the uploaded file for 2.5 seconds and then displaying all blobs in a table. For now we have used image tag to display all blobs considering that we are uploading the images.

To run this applicaiton, update the Azure Storage connction string in appsettings.Development.json or appsettings.json based in which mode you are running the project.

Then in UI, you have to run `npm start`.

Note: Please update api endpoint as 'target' in `proxy.config.json` file.  

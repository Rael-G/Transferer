# Transferer
API that supports file management operations (listing, searching, downloading, uploading, deletion) for authenticated users, with dedicated endpoints for authentication and authorization.

## Archives Controller:
This API provides functionality for managing user-specific archives. It requires authentication using a Bearer token. 
* **GET /Archives/list:** Retrieves a list of archives associated with the authenticated user.
* **GET /Archives/listall:** Retrieves a list of all archives for admin users.
* **GET /Archives/search/{name}:** Searches for archives by name for the authenticated user.
* **GET /Archives/download/{id}:** Downloads a specific archive by ID for the authenticated user.
* **GET /Archives/download/zip:** Downloads multiple archives in a zip file based on provided IDs for the authenticated user.
* **POST /Archives/upload:** Uploads new archives for the authenticated user.
* **DELETE /Archives/delete/{id}:** Deletes a specific archive by ID for the authenticated user.

## Auth Controller:
This controller handles user authentication and authorization. 
*** POST /login:** Logs in a user with provided credentials.
* **POST /signin:** Creates a new user with provided credentials.

## Users Controller: 
Manages user-related operations, accessible to admin users. 
* **GET /Users/get?id={id}:** Retrieves user details by ID for admin users.
* **GET /Users/search?name={name}:** Searches for users by name for admin users.
* **PUT /Users/edit:** Edits user details for the authenticated user.
* **DELETE /Users/delete?id={id}:** Deletes a user by ID for admin users.
                                                                                                         

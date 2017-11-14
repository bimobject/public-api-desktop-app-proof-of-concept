BIMobject API Demo Desktop Application: Authorization code with Proof key example.
============

An example of how you can use the authorization code with proof key flow from a Windows
desktop application to connect to the BIMobject API to search and list products, and download a file.
This example is configured to only download load rfa (Revit) files.

Introduction
------------

When developing native applications it is important to follow best practices,
this requires using a browser in the authorization process, but not an embedded browser.

This sample demonstrates how with the client ID and client secret from the developer.bimobject.com you can 
send an authorization request where a user can either either login in and approve the API usage or, if they are already
logged in, just need to approve the API scopes.
It shows that upon completing this consent process, you will recieve and authorization code using a local loopback
socket, and by exchanging that code for authorization tokens will allow the application to call the BIMobject API.


Documentation
--------------------
For more on the api and this authentication flow please apply for our API and see the documentation at:
https://developer.bimobject.com/app/#/docs#authorization-code-with-proof-key-pkce-


Getting Started
---------------
To use this demo you need to have your own credentials (client id, client secret) from developer.bimobject.com.
You also need to configure your API app settings to accept the redirect url http://127:0.0.1/ to allow the Authorization process 
to return information to the application. Please note that to use the API and get these details you require an account at
https://bimobject.com and to apply for access.

1. Open the solution file: `BIMobjectAPIDemoDesktopApp.sln`
2. Run the app.
3. When the app starts, take your client id, client secret and enter it in the fields corresponding fields.
4. Select a target folder for downloads.
5. Click the Login button.


Support
-------
If you have a question related to this samples or in general, please contact us through github.

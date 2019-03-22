# Manage custom polices in Azure AD B2C using Graph API
# Latest updates here: https://github.com/azure-ad-b2c/custom-policy-manager

This is a sample management tool for B2C Custom Policies.  [Custom policy](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-overview-custom) allows you to customize every aspect of the authentication flow.

## Features

This sample demonstrates the following:

* **Create** a custom policy
* **Update** a custom policy
* **Delete** a custom policy
* **List** all custom policies

![Policy Manager](/Images/b2cpolicymanager.PNG)

## Getting Started

### Prerequisites

This sample requires the following:
* [Visual Studio](https://www.visualstudio.com/en-us/downloads)
* [Azure AD B2C tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-get-started)

**NOTE: This API only accepts user tokens, and not application tokens. See more information below about Delegated Permissions.**

### Quickstart

#### Create global administrator

* A global administrator account is required to run admin-level operations and to consent to application permissions.  (for example: admin@myb2ctenant.onmicrosoft.com)

#### Setup and usage

1. Sign in to the [Azure Portal](https://portal.azure.com/) using your Global Admin account.
1. Select your Azure AD B2C directory from the directory filter.
2. Select the **Azure Active Directory Blade**.
3. Select **Application Registrations**, and create a new Application.
4. Select Type `Native`, and enter the redirect API: `https://b2capi.com`, create the App.
5. Select **Settings** - **Required Permissions** - **Add** - **Select An API**. Choose the `Microsoft Graph API`.
6. Select the permission **Read and write your organization's trust framework policies**.
7. Click **Save**, and click **Grant Permissions**.
1. Open and build the solution in Visual Studio. 
1. Run the application:
    
    a. Set the Tenant to your B2C tenant: something.onmicrosoft.com

    b. Set the V1 Graph App Id field to the Application Id from the App Registration created in the AAD Blade.

    c. Set the B2C Application Id to the App Id of an Application Registration created in the AAD B2C Blade.

    d. Set the reply url to a valid Reply URL set on the Application Registration referenced in the step above (AAD B2C App Registration).

1. Click Login and login with the Global Admin of your B2C tenant. It must be in the format user@something.onmicrosoft.com.

After logging in, any custom policies registered in the Identity Experience Framework at the portal or uploaded by this tool will be listed.

Select a Policy Folder that contains your XML files to upload them.

You can also open the working folder in VSCode by clicking Open Folder in VSCode.

## Questions and comments

Questions about this sample should be posted to [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-ad-b2c). Make sure that your questions or comments are tagged with [azure-ad-b2c].

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Resources

The sample uses the Active Directory Authentication Library (ADAL) for authentication. The sample demonstrates delegated admin permissions. (App only permissions are not supported yet)

**Delegated permissions** are used by apps that have a signed-in user present (in this case tenant administrator). For these apps either the user or an administrator consents to the permissions that the app requests and the app is delegated permission to act as the signed-in user when making calls to Microsoft Graph. Some delegated permissions can be consented to by non-administrative users, but some higher-privileged permissions require administrator consent.

See [Delegated permissions, Application permissions, and effective permissions](https://developer.microsoft.com/en-us/graph/docs/concepts/permissions_reference#delegated-permissions-application-permissions-and-effective-permissions) for more information about these permission types.

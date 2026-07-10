HTX586 split Admin + Driver pages

Thay thế/copy các file theo đúng thư mục:
- Components/Pages/Admin/DriverAccounts/Index.razor
- Components/Pages/Admin/DriverAccounts/Create.razor
- Components/Pages/Admin/DriverAccounts/Detail.razor
- Components/Pages/Admin/DriverAccounts/Edit.razor
- Components/Pages/Admin/Accounts/Index.razor
- Components/Pages/Admin/Accounts/Create.razor
- Components/Pages/Admin/Accounts/Detail.razor
- Components/Pages/Admin/Accounts/Edit.razor

Route sau khi tách:
- /admin/driver-accounts
- /admin/driver-accounts/create
- /admin/driver-accounts/{UserId}
- /admin/driver-accounts/{UserId}/edit
- /admin/accounts
- /admin/accounts/create
- /admin/accounts/{UserId}
- /admin/accounts/{UserId}/edit

Lưu ý:
1. Không để 2 file cùng @page route cũ.
2. DriverAccountDto và DriverAccountDetailDto cần có:
   - DriverSignatureIsActive
   - DriverSignatureInactiveAt
3. IDriverAccountService cần có UploadDriverSignatureAsync nếu dùng upload PNG riêng.
4. Nếu build bị file locked, kill HTX586CONTRACT.Web.exe trước khi build.

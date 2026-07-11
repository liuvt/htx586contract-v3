# Driver registration flow fix

- Owner/Admin can create Driver without signature.
- Driver must change password first, then create the one-time initial signature.
- Public self-registration creates an inactive Pending Driver account.
- Owner/Admin review pending requests at `/admin/driver-registrations`.
- Approval activates the account; rejection keeps it inactive.
- Startup adds the new registration columns to existing SQL Server databases.

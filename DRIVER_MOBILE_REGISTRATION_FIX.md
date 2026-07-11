# Driver mobile registration and approval notification

## Driver self-registration
- Mobile-first 4-step form at `/account/driver-register`.
- Step 1: username and password.
- Step 2: company/branch, full name, birth date, area, address and CCCD information.
- Step 3: driver-license number/class, issue date and expiry date.
- Expired licenses are blocked; licenses expiring within 30 days show a warning.
- Step 4 uses the shared `SignaturePad` component.
- The signature is stored under `master-signatures/drivers/{userId}` while the account remains locked and Pending.

## Owner/Admin review
- Added unseen fields: `RegistrationViewedAt`, `RegistrationViewedByUserId`.
- Navigation shows `Yêu cầu đăng ký mới (n)` for unseen Pending requests.
- Opening `/admin/driver-registrations/{userId}` marks the request as viewed, refreshes the badge, but keeps it Pending.
- The detail page shows account, company, personal information, CCCD, GPLX and signature before approval/rejection.

-- sql lite, mssql
use phishsim

alter table dbo.tenants
add RequireDomainVerification bit not null default 1
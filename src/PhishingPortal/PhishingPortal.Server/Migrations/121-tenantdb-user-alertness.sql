-- for sql lite, the other databases will have different column types

-- for all tenant db with this feature.

Alter Table CampaignLogs
Add IsReported INTEGER DEFAULT 0;

Alter Table CampaignLogs
Add ReportedBy VARCHAR(100) NULL;

Alter Table CampaignLogs
Add ReportedOn datetime NULL;

INSERT INTO `settings` (`Key`,`Value`,`CreatedBy`,`CreatedOn`)
VALUES('monitored_mail_account','phishing-mail@credentinfotech.com', 'tenantadmin', current_date());
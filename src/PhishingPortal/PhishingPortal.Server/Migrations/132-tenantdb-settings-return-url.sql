-- insert new tenant settings

insert into settings(`Key`, `Value`, CreatedBy, CreatedOn)
values ('return_urls','{"return_urls":["https://www.facebook.com/","https://twitter.com/","https://www.amazon.in/","https://www.myntra.in/"]}','tenantadmin', current_date() );

-- insert new tenant settings
insert into `t-20220619003439`.settings(`Key`, `Value`, CreatedBy, CreatedOn)
values ('return_urls','{"return_urls":[{"id":1,"url":"https://www.facebook.com/"},{"id":2,"url":"https://twitter.com/"},{"id":3,"url":"https://www.amazon.in/"}]}','tenantadmin', current_date() );

update `t-20220619003439`.settings
set `Value`= '{"return_urls":["https://www.facebook.com/","https://twitter.com/","https://www.amazon.in/","https://www.myntra.in/"]}'
where `Key` = 'return_urls';
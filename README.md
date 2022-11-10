# Introduction 
The PhishSims (Phishing Simulator) is an application for simulator phishing attacks.

# Getting Started

1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Release Notes

1. Sql Provider (v1.1.0)
	1. A new field named  added in the tenants table in the master database refer to migration script : 
		BlazorServerApp > Migrations > masterdb-add-new-column-to-tenant-entity.sql

# Build and Test
Describe and show how to build your code and run the tests. 

# Test after deployed the application    
1. Home
	1. All report tabs 
	2. Chart click
	3. Cross varify data on dahboard

2. Campaign
	1. All tabs 
	2. Check campaign edit button
 	3. Create new campaign with schedule and without schedule
	4. In new campaign change template type one by one and check if error throw for no template found

3. Template
	1. Create template 
	2. Check preview button
	3. check template edit button
	4. Check filters on data table

4. Setting
	1. Add redirect url and remove
	2. try to remove then re-add AD setting
	
5. Report
	1. Ckeck filters on data table
	
6. Login and logout test
7. Check demo request 

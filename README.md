# C# database sql database script manager, an alternative for migrations

The dbup-automation project is an alternative for the use of migrations.

It consists on the registration and execution of database scripts by any developer and can be integrated with azure.

At the time of execution, the SchemaVersions table is created, which manages which scripts have been executed and which still need to be executed.

These scripts can be run for any application or specific scripts can be defined per configured application.

All imported scripts must be defined as **Embedded Resource** and **Copy Always**, so that they can be copied at the time of build.

This project can become a phase of continuous deployment, where it can be defined in azure for a specific phase for which we want these scripts to be executed.



For the execution of this console application, you can, after building it, in the project folder in **_dbupa-application\bin\Debug\net5.0_** execute:

**./dbupa-application.exe -e staging -a application_two**


Console application Options:
_-e environment
-a application_

# README #

### Database Information ###
* User Table
	* id 			int				not null
	* UserName		varchar(25)		null
	* LastName		varchar(50)		null
	* FirstName		varchar(50)		null
	* Email			varchar(50)		null
	* Phone			char(10)		null
	* IsAdmin		bit				null
	* IsManager		bit				null
	* Password		varchar(25)		null
	
* Event
	* id			int				not null
	* StartTime		datetime		null
	* EndTime		datetime		null
	* Description	varchar(255)	null
	* NumVolunteers	int				null
	* Location		varchar(100)	null
	* Title			varchar(100)	null
	* Finalized 	bit				null
	
* Shift (StartTime would just be the hour the shift start, assuming 2 hour shifts)
	* id			int				not null
	* UserID		int				not null
	* EventID		int				not null
	* StartTime		int				null
	
### How do I get set up? ###
* Create the repo, schemas are above
	* Make sure the ids auto increment
* Need to setup an account to login from the application as. The current connection string uses User = sa and Password = password
   * Change sa password
      * In Management Studio, open Security > Logins then right click sa and select properties
      * In General, type the password in the password field and confirm it
   * Enable sa
      * In sa properties, select Status on the elft and click enable
   * Enable protocols
      * Open SQL Server Configuration Manager
      * On the left, open 'SQL Server Network Configuration', right click and activate all the protocols
   * Enable SQL Server authentication
      * Login via Windows Authentication
	  * Right click server and select Properties
	  * In security tab, select "SQL Server and Windows Authentication mode"
* Connect to local sql server express using 
	> sqlcmd -S localhost\sqlexpress -e -i db.txt
   * This may or may not actually work. It appears to insert the admin account if the database is created already.
   

Probably don't need to do below, it looks like the framework is included in the source.

* Install Entity Framework (this is needed for the calendar):
   * In the Visual Studio Solution Explorer, right click the solution, select 'Manage NuGet Packages for Solution...'
   * Search for 'Entity'
   * Install 'Entity framework'
* Install jQuery.FullCalendar:
   * In the Visual Studio Solution Explorer, right click the solution, select 'Manage NuGet Packages for Solution...'
   * Search for 'fullcalendar'
   * install 'jQuery.Fullcalendar

### Running the application ###
    * Navigate to the webite and login with user id and password
    * Users may view and sign up for events
    * Managers have user capability along with create user/event access and can manage user accounts
    * Admins have manager capability along with create manager/admin and can finalize events
   
### Documentation ###
For fullcalendar: https://fullcalendar.io/docs/usage/ :
   * Also: https://stackoverflow.com/questions/39098322/fullcalendar-jquery-plugin-not-appearing-mvc-4
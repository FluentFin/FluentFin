**Things that work:**
* Logging into the server
* Displaying the movies in the server
* Logging out of the server

**Things that don't work:**
* Literally everything else

To interested developers:
If you're facing issues logging into your server, make sure there aren't any slashes in the host field after your port number. Your host field should look something like this: "http://localhost:8096".

Postman tool was helpful to me in understanding Jellyfin's API. I've hardcoded the LibraryID string in MoviePage.xaml.cs. You will need to change its value for your movie library in order for movie fetching to work. You can find the libraryId in the URL of your movies page in jellyfin web.

![libraryId taken from Jellyfin Web Movies Page URL](screenshots/FluentFin_libraryId.png)

Alternatively, you can write logic to automatically fetch this value.

**Screenshots:**

Login Page with all fields empty:

![Login Page with all fields empty](screenshots/FluentFin_loginPage_Empty.png)

Error displayed when server is unreachable:

![Error displayed when server is unreachable](screenshots/FluentFin_loginPage_Error.png)

Login Page - Authentication in progress:

![Login Page - Authentication in progress](screenshots/FluentFin_loginPage_Login.png)

Movies Page:

![Movies Page](screenshots/FluentFin_moviesPage.png)

Sign Out Pop-up Window:

![Sign Out Pop-up Window](screenshots/FluentFin_signOut.png)

Movies Page Light Mode:

![Movies Page](screenshots/FluentFin_moviesPage_LightMode.png)

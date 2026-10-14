⚽ Tickets — Football Match Ticketing Platform

Tickets is a web application for discovering football matches, exploring upcoming sporting events, and purchasing tickets online. The platform provides a simple and user-friendly way for football fans to find matches and manage their ticket purchases.

From local league fixtures to European competitions and major football derbies, Tickets brings exciting football events together in one place.

✨ Features
🏟️ Football Events
Browse upcoming football matches from different leagues and competitions.
View event details, including teams, date, time, venue, and ticket prices.
Explore different ticket categories, such as Standard, VIP, and Premium.
View event images and match descriptions.
Check ticket availability and remaining capacity.
🎟️ Shopping Cart
Add tickets to the shopping cart.
Review selected tickets before checkout.
Update ticket quantities and remove items from the cart.
View the total price of selected tickets.
Proceed to purchase tickets without a separate reservation stage.
🔐 User Authentication
Register a new account and log in.
Support authenticated user functionality.
Separate administrative functionality from regular user operations.
🛠️ Event Administration
Create new football events.
Edit existing events.
Manage match dates, venues, ticket types, prices, and capacity.
Update event descriptions and images.
Maintain the event catalog.
🎨 User Interface
Modern and responsive design.
Intuitive navigation and an easy-to-use event catalog.
Clear event information and ticket purchasing interfaces.
Consistent styling and a user-friendly experience.
🖥️ Technologies

The project is built using the following technologies:

Technology	Purpose
C#	Application logic
ASP.NET Core MVC	Web application framework
Razor Views (.cshtml)	Dynamic page rendering
Entity Framework Core	Database access and management
SQLite	Database storage
HTML5	Page structure
CSS3	Styling and responsive layouts
JavaScript	Client-side interactivity
Bootstrap	Responsive UI components
📂 Project Structure
MVC.Intro/
├── Controllers/
│   ├── HomeController.cs
│   ├── EventController.cs
│   └── CartController.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbSeeder.cs
├── Models/
│   ├── Event.cs
│   ├── EventViewModel.cs
│   └── CreateEventViewModel.cs
├── Services/
│   └── EventService.cs
├── Views/
│   ├── Home/
│   ├── Event/
│   ├── Cart/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
├── Program.cs
├── appsettings.json
└── MVC.Intro.csproj


Note: The structure above is illustrative. The actual file and folder names may differ depending on the current implementation.

⚙️ Installation and Setup
Prerequisites

Before running the application, make sure you have installed:

.NET SDK
Visual Studio or Visual Studio Code
Git (optional, for cloning the repository)
1. Clone the Repository

Clone the project from GitHub:

git clone https://github.com/Denis4489/MVC.Intro.git

2. Navigate to the Project Directory
cd MVC.Intro/MVC.Intro


Make sure you are in the directory containing the MVC.Intro.csproj file before executing the following commands.

3. Restore Dependencies

Restore the project's required packages:

dotnet restore

4. Create the Database

To create the database and apply the existing Entity Framework Core migrations, execute the following command:

dotnet ef database update


This command creates the database if it does not already exist and applies all available migrations.

Make sure that the project contains the required migrations before running this command.

If the Entity Framework Core CLI tool is not installed, install it using:

dotnet tool install --global dotnet-ef


If the project does not contain any migrations yet, create the initial migration before updating the database:

dotnet ef migrations add InitialCreate
dotnet ef database update


Important: Only create an initial migration if one does not already exist. Back up any existing database before making schema changes.

5. Run the Application

Start the application with:

dotnet run


Open the local URL displayed in the terminal in your browser to access Tickets.

🗄️ Database

The application uses Entity Framework Core to manage its database.

The event data model can include the following information:

Event name and participating teams.
Match date and kickoff time.
Stadium or venue.
Ticket category.
Ticket price.
Total capacity and available tickets.
Event description and image.

The database supports event management and other application functionality according to the current implementation.

🚀 Future Improvements

Potential improvements for future versions include:

Integration with a real online payment provider.
User order history and purchased ticket management.
QR codes for digital tickets.
Advanced search and filtering by league, team, date, and price.
Dedicated pages for football leagues and competitions.
Stadium seating maps and seat selection.
Email confirmations after successful purchases.
Administrative dashboard with sales statistics.
Automated tests for the main application functionality.
🔒 Security

Security is an important part of an online ticketing platform.

Recommended practices include:

Restricting administrative actions to authorized users.
Validating user input on the server.
Verifying ticket prices and availability before completing a purchase.
Preventing purchases that exceed the remaining ticket capacity.
Using secure password handling through an established authentication framework.
Keeping sensitive configuration values out of source control.
🎯 Project Goals

The main goal of Tickets is to demonstrate how an ASP.NET Core MVC application can be used to develop a football ticketing platform with database integration, event management, authentication, and shopping cart functionality.

The project provides a foundation for further development into a complete online football ticket marketplace.

👨‍💻 Author

Denis

GitHub: Denis4489

📄 License

No license has been specified yet. If you plan to distribute or reuse the project publicly, consider adding an appropriate open-source license.

Tickets — Find your match. Experience the game. ⚽🎟️
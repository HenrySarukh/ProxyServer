CREATE DATABASE RailroadSellingCompany;

USE RailroadSellingCompany;

CREATE TABLE Trains (
    TrainID INT IDENTITY(1,1) PRIMARY KEY,
    TrainName VARCHAR(255) NOT NULL,
    TrainType VARCHAR(255) NOT NULL,
    NumberOfCarriages INT NOT NULL,
    MaximumCapacity INT NOT NULL
);

CREATE TABLE Routes (
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    RouteName VARCHAR(255) NOT NULL,
    DepartureStation VARCHAR(255) NOT NULL,
    ArrivalStation VARCHAR(255) NOT NULL,
    Distance INT NOT NULL,
    TravelTime INT NOT NULL
);

CREATE TABLE Schedule (
    ScheduleID INT IDENTITY(1,1) PRIMARY KEY,
    TrainID INT NOT NULL,
    RouteID INT NOT NULL,
    DepartureDate DATE NOT NULL,
    ArrivalDate DATE NOT NULL,
    DepartureTime TIME NOT NULL,
    ArrivalTime TIME NOT NULL,
    FOREIGN KEY (TrainID) REFERENCES Trains(TrainID),
    FOREIGN KEY (RouteID) REFERENCES Routes(RouteID)
);

CREATE TABLE Tickets (
    TicketID INT IDENTITY(1,1) PRIMARY KEY,
    ScheduleID INT NOT NULL,
    PassengerName VARCHAR(255) NOT NULL,
    PassengerContactNumber VARCHAR(255) NOT NULL,
    SeatNumber INT NOT NULL,
    TicketPrice INT NOT NULL,
    FOREIGN KEY (ScheduleID) REFERENCES Schedule(ScheduleID)
);

CREATE TABLE Stations (
    StationID INT IDENTITY(1,1) PRIMARY KEY,
    StationName VARCHAR(255) NOT NULL,
    StationLocation VARCHAR(255) NOT NULL,
    StationContactNumber VARCHAR(255) NOT NULL
);

USE MazanabeDB;

-- Users Table
CREATE TABLE Users (
    UserID INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(100) NOT NULL, -- Store hashed passwords in production
    Status VARCHAR(20) DEFAULT 'Active',
    CreatedDate DATETIME NOT NULL,
    SuspendedDate DATETIME NULL
);

-- Movies Table
CREATE TABLE Movies (
    MovieID INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(100) NOT NULL,
    Description TEXT,
    ReleaseYear INT,
    Director VARCHAR(100),
    DateAdded DATETIME NOT NULL,
    LastUpdated DATETIME NULL
);

-- Genres Table
CREATE TABLE Genres (
    GenreID INT AUTO_INCREMENT PRIMARY KEY,
    GenreName VARCHAR(50) NOT NULL UNIQUE,
    Description TEXT
);

-- Movie-Genre Relationship
CREATE TABLE MovieGenres (
    MovieID INT NOT NULL,
    GenreID INT NOT NULL,
    PRIMARY KEY (MovieID, GenreID),
    FOREIGN KEY (MovieID) REFERENCES Movies(MovieID) ON DELETE CASCADE,
    FOREIGN KEY (GenreID) REFERENCES Genres(GenreID) ON DELETE CASCADE
);

-- Questions Table
CREATE TABLE Questions (
    QuestionID INT AUTO_INCREMENT PRIMARY KEY,
    QuestionText TEXT NOT NULL,
    Category VARCHAR(50) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    LastUpdated DATETIME NULL
);

-- Watch History Table
CREATE TABLE WatchHistory (
    WatchID INT AUTO_INCREMENT PRIMARY KEY,
    UserID INT NOT NULL,
    MovieID INT NOT NULL,
    WatchDate DATETIME NOT NULL,
    WatchDurationMinutes INT,
    Completed TINYINT(1) DEFAULT 0,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (MovieID) REFERENCES Movies(MovieID) ON DELETE CASCADE
);

-- Ratings Table
CREATE TABLE Ratings (
    RatingID INT AUTO_INCREMENT PRIMARY KEY,
    UserID INT NOT NULL,
    MovieID INT NOT NULL,
    RatingValue INT NOT NULL,
    Comment TEXT,
    RatingDate DATETIME NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (MovieID) REFERENCES Movies(MovieID) ON DELETE CASCADE
);

-- Insert sample data
-- Sample Users
INSERT INTO Users (Username, Email, Password, Status, CreatedDate)
VALUES 
('admin', 'admin@mazanabe.com', 'hashed_password_here', 'Active', NOW()),
('john_doe', 'john@example.com', 'hashed_password_here', 'Active', NOW()),
('jane_smith', 'jane@example.com', 'hashed_password_here', 'Active', NOW());

-- Sample Genres
INSERT INTO Genres (GenreName, Description)
VALUES 
('Action', 'Fast-paced and exciting movies with intense sequences'),
('Comedy', 'Movies intended to make viewers laugh'),
('Drama', 'Character-driven stories that focus on emotional themes'),
('Horror', 'Movies designed to frighten and invoke fear'),
('Sci-Fi', 'Movies that explore speculative technological and scientific advancements');

-- Sample Movies
INSERT INTO Movies (Title, Description, ReleaseYear, Director, DateAdded)
VALUES 
('The Adventure Begins', 'An exciting journey through unknown lands', 2020, 'John Director', NOW()),
('Laugh Out Loud', 'A hilarious comedy about everyday life', 2021, 'Jane Director', NOW()),
('The Emotional Journey', 'A touching story about human connections', 2019, 'Bob Director', NOW()),
('Space Explorers', 'A journey to the far reaches of the galaxy', 2022, 'Alice Director', NOW());

-- Link Movies to Genres
INSERT INTO MovieGenres (MovieID, GenreID)
VALUES 
(1, 1), -- The Adventure Begins - Action
(2, 2), -- Laugh Out Loud - Comedy
(3, 3), -- The Emotional Journey - Drama
(4, 5); -- Space Explorers - Sci-Fi

-- Sample Questions
INSERT INTO Questions (QuestionText, Category, CreatedDate)
VALUES 
('What is your favorite movie genre?', 'General', NOW()),
('Who is your favorite actor?', 'General', NOW()),
('How often do you watch movies?', 'General', NOW()),
('What did you think of the ending of "The Adventure Begins"?', 'Movie specific', NOW());

-- Sample Watch History
INSERT INTO WatchHistory (UserID, MovieID, WatchDate, WatchDurationMinutes, Completed)
VALUES 
(2, 1, DATE_SUB(NOW(), INTERVAL 5 DAY), 120, 1),
(2, 2, DATE_SUB(NOW(), INTERVAL 3 DAY), 105, 1),
(3, 1, DATE_SUB(NOW(), INTERVAL 4 DAY), 118, 1),
(3, 3, DATE_SUB(NOW(), INTERVAL 2 DAY), 95, 0);

-- Sample Ratings
INSERT INTO Ratings (UserID, MovieID, RatingValue, Comment, RatingDate)
VALUES 
(2, 1, 5, 'Amazing movie with great action scenes!', DATE_SUB(NOW(), INTERVAL 5 DAY)),
(2, 2, 4, 'Very funny, made me laugh a lot.', DATE_SUB(NOW(), INTERVAL 3 DAY)),
(3, 1, 4, 'Good action movie, but the story could be better.', DATE_SUB(NOW(), INTERVAL 4 DAY)),
(3, 3, 5, 'So emotional, I cried at the end.', DATE_SUB(NOW(), INTERVAL 2 DAY));

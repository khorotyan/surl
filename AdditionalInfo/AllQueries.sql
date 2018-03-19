----------------------------- User -----------------------------

-- Create a new user
INSERT INTO [User] (Username, Email, Password) VALUES('Martin', 'martin@gmail.com', 'martin');

-- Get only one user
SELECT * FROM [User] WHERE Username = 'Mia';

-- Show the users with the most followers, and show whether the current user follows them
SELECT UserID, Username, COUNT(FollowedID) AS FollowerNum,
	MAX(CASE WHEN FollowingID = 6 THEN 1 ELSE 0 END) AS FollowingUser
	FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
	GROUP BY UserID, Username ORDER BY FollowingUser, FollowerNum DESC;

-- Search for a user
SELECT UserID, Username, COUNT(FollowedID) AS FollowerNum,
	MAX(CASE WHEN FollowingID = 6 THEN 1 ELSE 0 END) AS FollowingUser
	FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
	WHERE Username LIKE '%a%' 
	GROUP BY UserID, Username  ORDER BY FollowerNum DESC, LEN(Username);

-- Get all the users
SELECT * FROM [User];

----------------------------- Follow -----------------------------

-- Follow a user
INSERT INTO [Follow] (FollowingID, FollowedID) VALUES(3, 16);

-- Get the following table
SELECT * FROM [Follow];

-- Get all the people that a user follows (sorted based on follow length)
SELECT NewUsers.*, DATEDIFF(MINUTE, FollowDate, GETDATE()) AS FollowMins
	FROM (SELECT TOP 100 PERCENT UserID, Username, COUNT(FollowedID) AS FollowerNum 
		FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
		GROUP BY Username, UserID ORDER BY FollowerNum DESC) AS NewUsers
	LEFT JOIN [Follow] ON (FollowedID = NewUsers.UserID) WHERE FollowingID = 3 ORDER BY FollowDate DESC;

-- Get all the people that are following to a user
-- also show number of followers and whether the current user follows others
SELECT NewUsers.*, DATEDIFF(MINUTE, FollowDate, GETDATE()) AS FollowMins,
	CASE WHEN UserID IN 
		(SELECT FollowedID FROM [Follow] WHERE FollowingID = 6) 
		THEN 1 ELSE 0 END AS FollowingUser
	FROM (SELECT TOP 100 PERCENT UserID, Username, COUNT(FollowedID) AS FollowerNum 
		FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
		GROUP BY Username, UserID ORDER BY FollowerNum DESC) AS NewUsers
	LEFT JOIN [Follow] ON (FollowingID = NewUsers.UserID) WHERE FollowedID = 6 ORDER BY FollowDate DESC;

-- Search within your following list
SELECT NewUsers.*, DATEDIFF(MINUTE, FollowDate, GETDATE()) AS FollowMins
	FROM (SELECT TOP 100 PERCENT UserID, Username, COUNT(FollowedID) AS FollowerNum 
		FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
		GROUP BY Username, UserID ORDER BY FollowerNum DESC) AS NewUsers
	LEFT JOIN [Follow] ON (FollowedID = NewUsers.UserID) 
	WHERE FollowingID = 3 AND Username LIKE '%c%' ORDER BY FollowDate DESC;

-- Search within your followers list
SELECT NewUsers.*, DATEDIFF(MINUTE, FollowDate, GETDATE()) AS FollowMins,
	CASE WHEN UserID IN 
		(SELECT FollowedID FROM [Follow] WHERE FollowingID = 6) 
		THEN 1 ELSE 0 END AS FollowingUser
	FROM (SELECT TOP 100 PERCENT UserID, Username, COUNT(FollowedID) AS FollowerNum 
		FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
		GROUP BY Username, UserID ORDER BY FollowerNum DESC) AS NewUsers
	LEFT JOIN [Follow] ON (FollowingID = NewUsers.UserID) 
	WHERE FollowedID = 6 AND Username LIKE '%c%' ORDER BY FollowDate DESC;

-- Unfollow a user
DELETE FROM [Follow] WHERE FollowingID = 6 AND FollowedID = 12;

----------------------------- Questions(Comments, Likes, Views) -----------------------------

-- Create a question (also create the tags)
INSERT INTO [Question] (UserID, QuestionText, Description, Tags) 
	VALUES(9, 'Some Question', 'Some Description', 'tag1 tag2 tag3');

-- Edit a question (edit question, description, or tags)
UPDATE [Question] SET QuestionText = 'Modified question', Description = 'Modified description', Tags = 'newtag some_tag another-tag' WHERE QuestionID = 3;

-- Delete a question
DELETE FROM [Question] WHERE QuestionID = 3;

-- Sort questions by / Popular / Recent / Friend / Own / Trending / Unanswered / Unverified /
-- Also show likes, answers, views, and whether the question is verified or not
--- Popular Questions
SELECT * FROM
	(SELECT TOP 100 PERCENT Q.*,
		(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
 		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		FROM [Question] AS Q 
		ORDER BY Likes DESC) AS NewQuestion;
--- Recent Questions
SELECT * FROM
	(SELECT TOP 100 PERCENT Q.*,
		(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		FROM [Question] AS Q 
		ORDER BY Q.PostDate DESC) AS NewQuestion;
--- Friend Questions
SELECT * FROM
	(SELECT TOP 100 PERCENT Q.*,
		(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		FROM [Question] AS Q
		WHERE Q.UserID IN (SELECT FollowedID FROM [Follow] WHERE FollowingID = 3)
		ORDER BY Q.PostDate DESC) AS NewQuestion;
--- Own Questions
SELECT * FROM
	(SELECT TOP 100 PERCENT Q.*,
		(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		FROM [Question] AS Q 
		WHERE Q.UserID = 16
		ORDER BY Q.PostDate DESC) AS NewQuestion;
--- Trending Questions (Weekly)
SELECT * FROM
	(SELECT TOP 100 PERCENT
		Q.*, 
		(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,	
		(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified,
		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID 
			AND ViewTime > DATEADD(HOUR, -24*7, GETDATE())) AS NewViews,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID 
			AND LikeTime > DATEADD(HOUR, -24*7, GETDATE())) AS NewLikes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID
			AND AnswerDate > DATEADD(HOUR, -24*7, GETDATE())) AS NewAnswers
		FROM [Question] AS Q) AS NewQuestion
	ORDER BY (0.2 * NewViews + 0.3 * NewAnswers + 0.5 * NewLikes) DESC;
--- Unanswered Questions
SELECT * FROM 
	(SELECT TOP 100 PERCENT 
		Q.*, 
		(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		FROM [Question] AS Q LEFT JOIN [Comment] AS C ON (Q.QuestionID = C.QuestionID) 
		ORDER BY Q.PostDate DESC) AS NewQuestion
	WHERE Answers = 0;
--- Unverified Questions
SELECT * FROM 
	(SELECT TOP 100 PERCENT 
		Q.*, 
		(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		FROM [Question] AS Q 
		ORDER BY Q.PostDate DESC) AS NewQuestion
	WHERE Verified = 0;

-- Search for a question in the question, tag, username and description fields 
SELECT * FROM
	(SELECT TOP 100 PERCENT
		Q.*, 
		(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		FROM [Question] AS Q) AS NewQuestion
	WHERE CONCAT_WS('', QuestionText, Tags, Username, Description) LIKE '%rob%';

-- Get a specific question
SELECT *,
	(SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
	(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
	(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes
	FROM [Question] AS Q,
	(SELECT COUNT(*) AS Answers, ISNULL(MAX(CAST(Verified AS INT)), 0) AS Verified 
		FROM [Comment] WHERE QuestionID = 4) AS C 
	WHERE Q.QuestionID = 4;

-- View a question
INSERT INTO [View] (QuestionID, UserID) VALUES (14, 4);

-- Like/Dislike a question
INSERT INTO [LikeQuestion] (QuestionID, UserID, LikeValue) VALUES (13, 12, 1);

-- Create a comment
INSERT INTO [Comment] (QuestionID, UserID, CommentText) VALUES (2, 10, 'Another Comment');

-- Edit a comment 
UPDATE [Comment] SET CommentText = 'Modified Comment' WHERE CommentID = 1;

-- Delete a comment
DELETE FROM [Comment] WHERE CommentID = 1;

-- Like/Dislike a comment
INSERT INTO [LikeComment] (CommentID, UserID, LikeValue) VALUES (11, 13, 1);

-- Verify/Unverify a comment 
UPDATE [Comment] SET Verified = 1 WHERE CommentID = 32;

-- Get comments of a question sorted by / Popular / Recent / 
--- Popular comments
SELECT Comment.*, 
	(SELECT Username FROM [User] WHERE UserID = Comment.UserID) AS Username,
	(SELECT ISNULL(SUM(LikeValue), 0) FROM LikeComment WHERE CommentID = Comment.CommentID) 
	AS Likes FROM [Comment] ORDER BY Likes DESC;
--- Recent comments
SELECT Comment.*, 
	(SELECT Username FROM [User] WHERE UserID = Comment.UserID) AS Username,
	(SELECT ISNULL(SUM(LikeValue), 0) FROM LikeComment WHERE CommentID = Comment.CommentID) 
	AS Likes FROM [Comment] ORDER BY AnswerDate;

-- Get all questions
SELECT * FROM [Question];

-- Get all comments
SELECT * FROM [Comment];

-- Get all Views
SELECT * FROM [View];

-- Get all likes
SELECT * FROM [LikeQuestion];
SELECT * FROM [LikeComment];

-- Clear tables
DELETE FROM [Question]

----------------------------- ___ Performance Measures ___ -----------------------------

DECLARE @t1 DATETIME2;
DECLARE @t2 DATETIME2;

SET @t1 = GETDATE();
SELECT Q.*, COUNT(C.AnswerDate) AS Answers, ISNULL(MAX(CAST(C.Verified AS INT)), 0) AS Verified,
	(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
	(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes
	FROM [Question] AS Q LEFT JOIN [Comment] AS C ON (Q.QuestionID = C.QuestionID) 
	GROUP BY Q.Description, Q.PostDate, Q.QuestionID, Q.QuestionText, Q.UserID
	ORDER BY Likes DESC;

SET @t2 = GETDATE();
SELECT DATEDIFF(nanosecond,@t1,@t2) AS Nano1;


SET @t1 = GETDATE();

SELECT Q.*,
	(SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
	(SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
	(SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
	(SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
	FROM [Question] AS Q ORDER BY Likes DESC;

SET @t2 = GETDATE();
SELECT DATEDIFF(nanosecond,@t1,@t2) AS Nano2;
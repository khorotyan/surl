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

-- Search for a user, search for Mia
SELECT UserID, Username, COUNT(FollowedID) AS FollowerNum,
	MAX(CASE WHEN FollowingID = 6 THEN 1 ELSE 0 END) AS FollowingUser
	FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
	WHERE Username LIKE '%mia%' 
	GROUP BY UserID, Username  ORDER BY FollowerNum DESC, LEN(Username);

-- Get all the users
SELECT * FROM [User];

----------------------------- Follow -----------------------------

-- Follow a user, Kevin Follows Mia
INSERT INTO [Follow] (FollowingID, FollowedID) VALUES(6, 12);

-- Get the following table
SELECT * FROM [Follow];

-- Get all the people that a user follows (sorted based on follow length), get all the people Kevin follows
SELECT NewUsers.*, DATEDIFF(MINUTE, FollowDate, GETDATE()) AS FollowMins
	FROM (SELECT TOP 100 PERCENT UserID, Username, COUNT(FollowedID) AS FollowerNum 
		FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
		GROUP BY Username, UserID ORDER BY FollowerNum DESC) AS NewUsers
	LEFT JOIN [Follow] ON (FollowedID = NewUsers.UserID) WHERE FollowingID = 6 ORDER BY FollowDate DESC;

-- Get all the people that are following to a user, people that follow Kevin
-- also show number of followers and whether the current user follows others
SELECT NewUsers.*, DATEDIFF(MINUTE, FollowDate, GETDATE()) AS FollowMins,
	CASE WHEN UserID IN 
		(SELECT FollowedID FROM [Follow] WHERE FollowingID = 6) 
		THEN 1 ELSE 0 END AS FollowingUser
	FROM (SELECT TOP 100 PERCENT UserID, Username, COUNT(FollowedID) AS FollowerNum 
		FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
		GROUP BY Username, UserID ORDER BY FollowerNum DESC) AS NewUsers
	LEFT JOIN [Follow] ON (FollowingID = NewUsers.UserID) WHERE FollowedID = 6 ORDER BY FollowDate DESC;

-- Unfollow a user, Kevin unfollows Mia 
DELETE FROM [Follow] WHERE FollowingID = 6 AND FollowedID = 12;

----------------------------- Question -----------------------------

-- Create a question
INSERT INTO [Question] (UserID, QuestionText, Description) VALUES(16, 'Another Question', 'Another Description');

-- Delete a question, also clear its tags and comments
DELETE FROM [Tag] WHERE QuestionID = 5;
DELETE FROM [Comment] WHERE QuestionID = 4;
DELETE FROM [Question] WHERE QuestionID = 5;

-- Get all the questions
SELECT * FROM [Question];

-- Like/Dislike and View a question
UPDATE [Question] SET Likes = Likes+1 WHERE QuestionID = 1;
UPDATE [Question] SET Likes = Likes-1 WHERE QuestionID = 1;
UPDATE [Question] SET Views = Views+1 WHERE QuestionID = 1;

-- Update the question and the description
UPDATE [Question] SET QuestionText = 'Modified question', Description = 'Modified description' WHERE QuestionID = 1;

-- Get a question, and show whether it is verified
SELECT Question.*, (SELECT COUNT(Verified) FROM [Comment] WHERE QuestionID = 7 AND Verified = 1) AS Verified
	FROM [Question] WHERE QuestionID = 7;

-- Sort the questions by Most Popular / Recent / Unanswered / Own questions / Followers / Unverified
-- Most Popular
SELECT Question.*, (SELECT COUNT(Verified) FROM [Comment] WHERE QuestionID = Question.QuestionID AND Verified = 1) AS Verified
	FROM [Question] ORDER BY Likes DESC ;
-- Recent
SELECT Question.*, (SELECT COUNT(Verified) FROM [Comment] WHERE QuestionID = Question.QuestionID AND Verified = 1) AS Verified
	FROM [Question] ORDER BY PostDate DESC;
-- Unanswered
SELECT Question.*, (SELECT COUNT(Verified) FROM [Comment] WHERE QuestionID = Question.QuestionID AND Verified = 1) AS Verified
	FROM [Question] WHERE Answers = 0;
-- Own questions
SELECT Question.*, (SELECT COUNT(Verified) FROM [Comment] WHERE QuestionID = Question.QuestionID AND Verified = 1) AS Verified
	FROM [Question] WHERE UserID = 17;
-- Followers
SELECT Question.*, (SELECT COUNT(Verified) FROM [Comment] WHERE QuestionID = Question.QuestionID AND Verified = 1) AS Verified
	FROM [Question] WHERE UserID IN (SELECT FollowedID FROM [Follow] WHERE FollowingID = 6);
-- Unverified
SELECT * FROM 
	(SELECT TOP 100 PERCENT Question.*, 
		(SELECT COUNT(Verified) FROM [Comment] WHERE QuestionID = Question.QuestionID AND Verified = 1) AS Verified
		FROM [Question] ORDER BY PostDate DESC) AS NewQuestions
	WHERE NewQuestions.Verified = 0;

----------------------------- Tag -----------------------------

-- Add / Remove a tag from the question
INSERT INTO [Tag] (QuestionID, TagText) VALUES (1, 'SomeTag1'), (1, 'SomeTag2'), (1, 'SomeTag3');
DELETE FROM [Tag] WHERE QuestionID = 1 AND TagText = 'SomeTag4';

-- Show the most frequently used tags
SELECT TagText, COUNT(TagText) AS TagNum FROM [Tag] GROUP BY TagText ORDER BY TagNum DESC;

-- Show all tags
SELECT * FROM [Tag];

----------------------------- Comment -----------------------------

-- Answer to a question
INSERT INTO [Comment] (QuestionID, UserID, CommentText) VALUES (6, 7, 'Another answer');
UPDATE [Question] SET Answers = Answers+1 WHERE QuestionID = 6;

-- Show all the questions
SELECT * FROM [Comment];

-- Delete an answer
DELETE FROM [Comment] WHERE CommentID = 2;
UPDATE [Question] SET Answers = Answers-1 WHERE QuestionID = 6;

-- Like a comment
UPDATE [Comment] SET Likes = Likes+1 WHERE CommentID = 4;

-- Verify / Unverify an answer
UPDATE [Comment] SET Verified = 1 WHERE CommentID = 6;
UPDATE [Comment] SET Verified = 0 WHERE CommentID = 6;

-- Sort Questions by Date / Likes, get verified answer first
SELECT * FROM [Comment] WHERE QuestionID = 6 ORDER BY Verified DESC, AnswerDate DESC;
SELECT * FROM [Comment] WHERE QuestionID = 6 ORDER BY Verified DESC, Likes DESC;
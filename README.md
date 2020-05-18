# lambda-mitch

A serverless .NET application that displays a randomly selected Mitch Hedberg joke.

When invoked by an HTTP request, JokeFunction selects a random joke from a list of jokes stored in an S3 bucket and inserts that joke into the HTML that is returned.

https://mitch.daydreamtomb.com/
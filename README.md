Reliability:

I have made my system more reliable by implementing a RetryPolicy. When PostService creates a post, it has to know if the ID of the user is valid. It sends an api call to UserService to validate the ID. If the UserService does not respond, PostService will try again up to 3 times.

Security:

I have implemented North-South security, in my Docker Compose setup, by removing all the ports and only keeping the ApiGateway port, which in turn will do all the routing. I then put everything on the same network, so everything call communicate with each other. I also implemented Authentication with Jwt Bearer Tokens.
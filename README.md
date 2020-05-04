# Powerplant Coding Challenge
This project is created in visual studio 2019 using .NET Core 3.1 framework and Angular8

## Required tools
1) NodeJs
2) Visual studio

## Installation

1) Unzip the Powerplant_Coding_Challenge.zip

2) Open the solution with Visual Studio

3) Click the start IIS Express button 

4) Visual studio will automatically download the required node_modules (if this for some reason is not happening see below for more info)

5) Once the download is finished, the application will start

## Angular 8
For having some visual feedback I also created a front-end portal with Angular8

You can send a POST request with an editable body object

Several test scenarios can be created using the extra buttons

You can see what the browser received via websockets

## Docker
There is a Dockerfile along with the implementation to allow deploying your solution quickly.

1) Run the command in the parent directory

```
docker build -f "Powerplant Coding Challenge"/Dockerfile -t coding-challenge .
```

This can take several minutes
2) When the build is done, start the container with the following command:
```
docker run --name myapp --rm -it -p 8000:80 coding-challenge
```
3) You can now visit the application on localhost:8080

## Websocket
There is a websocket server connection that will emit after every post the input of the POST together with the response to every client connected on the websocket.

## Node Modules
Normally visual studio is automatically downloading the required node_modules using the package.json file.
If this is not happening you will need to download it yourself using npm.

1) Open command prompt
2) Change the directory to the folder "ClientApp"
3) Run the following command to start downloading:

```
npm install
```
4) After the download is finished, you can start the solution

import * as express from 'express';
import * as http from 'http';
import { AddressInfo } from 'net';
import * as webSocket from 'ws';

const app = express();
const server = http.createServer(app);
const wss = new webSocket.Server({ server });

wss.on('connection', (ws: WebSocket) => {

    console.log("client connected.");

    ws.onmessage = (event : MessageEvent)=>{

        console.log(event.data);
    }

});

server.listen(process.env.PORT || 8080, ()=>{
    console.log("Server start at port "+(server.address() as AddressInfo).port);
});
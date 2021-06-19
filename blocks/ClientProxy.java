package fuckoffjava;

import java.io.*;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import javax.imageio.ImageIO;

import lenz.htw.blocks.Move;
import lenz.htw.blocks.net.NetworkClient;

public class ClientProxy extends Thread
{
	public static void main(String[] args) {
		int proxyCount = 3;
		String name = "Test";
		String logo = "/home/nojavapls/logo.jpg";
		
		ClientProxy[] proxies = new ClientProxy[proxyCount];
		for(int i = 0; i < proxies.length; ++i)
			proxies[i] = new ClientProxy("127.0.0.1", name + " (" + i + ")", logo, 55555 + i);
		
		for(ClientProxy proxy : proxies)
			proxy.start();
		
		for(ClientProxy proxy : proxies)
			try {
				proxy.join();
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
	}

	private String name;
	
	private String ip;
	
	private String logo;
	
	private int forwardPort;
	
	public ClientProxy(String ip, String name, String logo, int forwardPort)
	{
		this.name = name;
		this.ip = ip;
		this.logo = logo;
		this.forwardPort = forwardPort;
	}
	
	public void run()
	{
		try {
			System.out.println("Waiting for client...");
			
			ServerSocket serverSocket = new ServerSocket(forwardPort);
			Socket clientSocket = serverSocket.accept();
			
			System.out.println(clientSocket.getInetAddress().getHostAddress() + " connected on port " + forwardPort);
			
			NetworkClient client = new NetworkClient(this.ip, this.name, ImageIO.read(new File(this.logo)));
			
			int playerId = client.getMyPlayerNumber();	
			
			int[] msg = {
				playerId,
				client.getTimeLimitInSeconds(),
				client.getExpectedNetworkLatencyInMilliseconds(),
				-1
			};
			
			OutputStream output = clientSocket.getOutputStream();
			InputStream input = clientSocket.getInputStream();
			
			for(int i : msg)
				writeInt(output, i);
			
			Move move = null;

			while (true) {
				while ((move = client.receiveMove()) != null)
				{
					if(move.player == playerId)
						continue;
					
					msg = new int[] {
						move.player,
						move.first == 255 ? -1 : move.first,
						move.second == 255 ? -1 : move.second,
						move.delete
					};
					
					System.out.print("Received move: ");
					for(int i : msg)
						System.out.print(i + ", ");
					System.out.print("\n");
					
					for(int i : msg)
						writeInt(output, i);
				}
				
				msg = new int[] {
					playerId,
					-1,
					-1,
					-1
				};
				
				for(int i : msg)
					writeInt(output, i);
				
				for(int i = 0; i < msg.length; ++i)
					msg[i] = readInt(input);
				
				client.sendMove(new Move(msg[0], msg[3], msg[1], msg[2]));
			
				System.out.print("Send move: ");
				for(int i : msg)
					System.out.print(i + ", ");
				System.out.print("\n");
			}
			
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (RuntimeException e) {
			System.out.println("Dam dam dam...");
		}
		
	}
	
	public static void writeInt(OutputStream stream, int value) throws IOException
	{
		ByteBuffer buffer = ByteBuffer.allocate(4);
		buffer
			.order(ByteOrder.LITTLE_ENDIAN)
			.putInt(value);
		stream.write(buffer.array());
	}
	
	public static int readInt(InputStream stream) throws IOException
	{
		byte[] bytes = new byte[4];
		int received = 0;
		while(received < bytes.length)
			received += stream.read(bytes, received, bytes.length - received);
		
		ByteBuffer buffer = ByteBuffer.allocate(4);
		buffer
			.order(ByteOrder.LITTLE_ENDIAN)
			.put(bytes)
			.flip();
		
		int value = buffer.getInt();
		
		return value == 255 ? -1 : value;
	}
}

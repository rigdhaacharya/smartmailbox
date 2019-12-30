/*  
    Arduino code to work with PIR module, RFID reader and ESP8266 module
    PIR sensor code based on: http://RandomNerdTutorials.com/pirsensor
*/
#include <SPI.h>
#include <RFID.h>
#include <SoftwareSerial.h>
#define RX 2
#define TX 3
SoftwareSerial esp8266(RX,TX);
#define BUFFER_SIZE 512

char buffer[BUFFER_SIZE];

String data; //data for the POST request
String server = "10.0.0.223"; // www.example.com
String uri = "/"; //server side script to call

/* Define the DIO used for the SDA (SS) and RST (reset) pins. */
#define SDA_DIO 9
#define RESET_DIO 8

/* Create an instance of the RFID library */
RFID RC522(SDA_DIO, RESET_DIO); 
int led = 13;                // the pin that the LED is atteched to
int sensor = 7;              // the pin that the sensor is atteched to
int state = LOW;             // by default, no motion detected
int val = 0;                 // variable to store the sensor status (value)

 
void setup() {
  pinMode(led, OUTPUT);      // initalize LED as an output
  pinMode(sensor, INPUT);    // initialize sensor as an input
  Serial.begin(9600);        // initialize serial
  SPI.begin(); 
  /* Initialise the RFID reader */
  RC522.init();
  esp8266.begin(115200);
  setupWiFi();
 Serial.println("connected to wifi");
}

// By default we are looking for OK\r\n
byte wait_for_esp_response(int timeout) {
  unsigned long t=millis();
  bool found=false;
  int i=0;
  while(millis()<t+timeout) {
    if(esp8266.available()) {
      buffer[i++]=esp8266.read();
      }
    }
  
  buffer[i]=0;
  Serial.print(buffer);
  return found;
}

void sendtagid(String number){
  esp8266.println("AT+CIPSTART=\"TCP\",\"10.0.0.223\",80");//start a TCP connection.
  delay(1000);
  wait_for_esp_response(1000);
  
  delay(1000);

  String postRequest = String("GET ");
  postRequest = postRequest + uri + "?id="+number+" HTTP/1.1\r\n" +
  "Host: " + server  +":80"+ "\r\n";
    String sendCmd = "AT+CIPSEND=";
    esp8266.print(sendCmd);
    esp8266.println(postRequest.length() );
    delay(1000);
    
     Serial.println("Sending..");
    esp8266.print(postRequest);

    Serial.print("Request length: ");
    Serial.println(postRequest.length());
    Serial.println("Request Sent:");
    Serial.println(postRequest);

    if( esp8266.find("SEND OK")) {
      Serial.println("Packet sent");
      while (esp8266.available()) {
        String tmpResp = esp8266.readString();
        Serial.println(tmpResp);
      }

      
    }
    // close the connection
    esp8266.println("AT+CIPCLOSE");
}

void setupWiFi() {
  // try empty AT command
  esp8266.println("AT");
  wait_for_esp_response(1000);

  // set mode 1 (client)
  esp8266.println("AT+CWMODE=1");
  wait_for_esp_response(1000); 

  // reset WiFi module
  esp8266.println("AT+RST");
  wait_for_esp_response(1500);
  delay(3000);
  esp8266.println("AT+CWJAP=\"insert_network_name_here\",\"insert_network_password_here\"");
  wait_for_esp_response(5000);
    
}


void loop(){
  val = digitalRead(sensor);   // read sensor value
  if (val == HIGH) {           // check if the sensor is HIGH
    digitalWrite(led, HIGH);   // turn LED ON
      byte i;
      int read=10; //read upto 10 tags at a time
    if (state == LOW) {
      Serial.println("Motion detected!"); 
      Serial.println("Enable RFID reader");
       state = HIGH;       // update variable state to HIGH
      /* Has a card been detected? */
     while(read>0){
    if (RC522.isCard())
    {
      read--;
      /* If so then get its serial number */
      RC522.readCardSerial();
  
      Serial.println("Card detected:");
      
      /* Output the serial number to the UART */
      String number="";
      for(i = 0; i <= 4; i++)
      {
        number+=RC522.serNum[i];
      }
      Serial.println("serial number :" + number);
      sendtagid(number);
      Serial.println();
    }
    delay(200);
    }
  }
  } 
  else {
      digitalWrite(led, LOW); // turn LED OFF
      delay(200);             // delay 200 milliseconds 
      
      if (state == HIGH){
        Serial.println("Motion stopped!");
        state = LOW;       // update variable state to LOW
    }
  }
}

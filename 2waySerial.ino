bool enablerun = true;
bool newData = false;
String sin1, sin2; //sin = Serial INput
char receivedCommand; //received character as a command
 
void setup()
{
  Serial.begin(9600); //define baud rate
  Serial.println("Testing Serial Commands"); //print a message
  Serial.println("d = send formatted data");
  Serial.println("s = stop sending data");
  Serial.println("----------------------------");
}
 
void loop()
{
  CheckSerialSwitchCase(); //This is with Switch - Case
  sendFormattedData(); //This is a function
 
}
 
void CheckSerialSwitchCase() //Checking the serial port via Switch-Case
{
  if (Serial.available()) { //If there are bytes/characters coming through the serial port
 
    char commandcharacter = Serial.read(); //read the character for the command
   
    switch (commandcharacter) { //we read a character which can
   
    case 'd': //we request formatted data
     
      enablerun = true; // this enables the function for sending formatted data      
 
    break;
 
    case 's': //s = stop; we make enablerun false, so we stop receiving data.
 
      enablerun = false;
      Serial.println("Stopped!");
 
    break;
    }
  }
}
 
void sendFormattedData()// we create a random series of data which will be transferred to the serial terminal formatted
{
  if (enablerun == true)
  {
    for (float i = 0.0; i < 2 * PI; i += 0.1) {  // cosine wave
      Serial.print(sin(i));  Serial.print(",");  // sine wave
      Serial.println(cos(i)); //Serial.print(",");
      //Serial.println(i <= 2 ? -4 : 4);      // square wave
      delay(100);
   }
  }  
 
}
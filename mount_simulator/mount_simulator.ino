
int Mntposition = 0;
int wrapCW = 0;
int wrapCCW = 0;
unsigned long int delayTime  = 0;
int dec = 0;
int driveCommand = 0;
int driveDir = 0;
int i;
int outb = 0;
void setup() {
  // put your setup code here, to run once:
  for (int i = 0; i < 12; i++)
    {
      pinMode(i, OUTPUT);
      digitalWrite (i, 0);
    }
    pinMode(13,INPUT);
//    Serial.begin(9600);
//    Serial.print("\r");
}

void loop() {

  driveCommand = analogRead(A0);
  driveDir = digitalRead (13);
  
//  Serial.print (F("Command "));
//  Serial.print (driveCommand);
//  Serial.print(F("\r\n"));
  if (driveCommand < 10)
  {
    delayTime = 100;
  }
  else
  {
  delayTime  = (1024L - driveCommand)*4L; //44ms / count at 2 deg/sec
  }
    
//  Serial.print (F("Delay "));
//  Serial.print (delayTime);
//  Serial.print(F("\r\n"));
  
  while (delayTime > 1000)
  {
    delay(1000);
    delayTime -= 1000;
  }
  delay(delayTime);
  delayTime = 0;
  
  dec = (driveDir == 1)?-1:1;
  if (driveCommand > 10)
    Mntposition = Mntposition + dec;
  
  if (Mntposition < 0)
  {
    if (wrapCW == 1)
    {
      wrapCW = 0;
    }
    else
    {
    wrapCCW = 1;
    }
    
    Mntposition += 4095;
  }
  if (Mntposition >= 4096)
  {
    Mntposition -= 4096;
    if (wrapCCW == 1)
    {
      wrapCCW = 0;
    }
    else 
    {
      wrapCW = 1;
    }
  }

  for (i = 0; i < 12; i++)
  {
    outb = (Mntposition >> i)&0x0001;
    digitalWrite (i,(outb==1?HIGH:LOW));
  }
 //   Serial.print (F("Position "));
 //   Serial.print (Mntposition);
 //   Serial.print(F("\r\n"));
  
}

#include <CmdMessenger.h>
//#include <Logging.h>  // currently all usings of logging library are commented out
#include "global.h"
#include "motor.h"
#include "input.h"


//#define LOGLEVEL LOG_LEVEL_DEBUG




void setup()
{
  delay(2000);
  
  messenger.printLfCr();
  AttachCommandCallbacks();
  messenger.printLfCr();
  OnStatus();
  //Log.Init(LOGLEVEL, SERIAL_SPEED);

}

void loop()
{
  messenger.feedinSerialData();
  left.Tick();
  right.Tick();
}

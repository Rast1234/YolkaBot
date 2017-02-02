#include <CmdMessenger.h>
#include "global.h"
#include "input.h"


void AttachCommandCallbacks()
{
  messenger.attach(OnUnknownCommand);
  messenger.attach(cmd_left, OnLeft);
  messenger.attach(cmd_right, OnRight);
  messenger.attach(cmd_stop, OnStop);
  messenger.attach(cmd_status, OnStatus);
}

void OnStatus()
{
  messenger.sendCmdStart(cmd_status);
  messenger.sendCmdArg(left.GetSpeed());
  messenger.sendCmdArg(right.GetSpeed());
  messenger.sendCmdEnd();
}
void OnUnknownCommand()
{
  messenger.sendCmd(cmd_status,"Unknown command received!");
}

void OnLeft()
{
  int speed = messenger.readInt32Arg();
  left.SetSpeed(speed);
  OnStatus();
}

void OnRight()
{
  int speed = messenger.readInt32Arg();
  right.SetSpeed(speed);
  OnStatus();
}

void OnStop()
{
  left.Stop();
  right.Stop();
  OnStatus();
}


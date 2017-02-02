#ifndef input_h
#define input_h

enum commands
{
  cmd_left,
  cmd_right,
  cmd_stop,
  cmd_status
};

void OnStatus();
void OnUnknownCommand();
void OnLeft();
void OnRight();
void OnStop();
void AttachCommandCallbacks();

#endif

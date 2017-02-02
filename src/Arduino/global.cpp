#include <CmdMessenger.h>
#include "motor.h"

extern CmdMessenger messenger = CmdMessenger(Serial);
extern int SERIAL_SPEED = 115200;
extern Motor left = Motor(1, 2, 5);
extern Motor right = Motor(3, 4, 6);


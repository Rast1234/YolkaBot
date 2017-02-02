#include <Arduino.h>
#include <Logging.h>
#include "motor.h"

const int Motor::PWM_MAX = 127;      // i am trying not to kill 6V-motors with 12V-battery
                                     // so will use half of maximum possible value (255/2)

const int Motor::SPEED_STEP = 10;    // will change PWM for this value per tick

const int Motor::TICK_MS = 10;       // change speed every TICK_MS milliseconds

Motor::Motor(byte forward, byte back, byte pwm)
{
  pinMode(pwm, OUTPUT);
  pinMode(forward, OUTPUT);
  pinMode(back, OUTPUT);

  PWM = pwm;
  FWD = forward;
  BCK = back;
  
  this->currentSpeed = 0;
  this->desiredSpeed = 0;
  this->desiredPercentage = 0;
  this->changeTime = 0;
}

void Motor::Stop()
{
  digitalWrite(PWM, LOW);
  digitalWrite(FWD, LOW);
  digitalWrite(BCK, LOW);
  
  this->currentSpeed = 0;
  this->desiredSpeed = 0;
  this->desiredPercentage = 0;
  this->changeTime = 0;
}

void Motor::Brake()
{
  digitalWrite(PWM, HIGH);
  digitalWrite(FWD, HIGH);
  digitalWrite(BCK, HIGH);
  
  this->currentSpeed = 0;
  this->desiredSpeed = 0;
  this->desiredPercentage = 0;
  this->changeTime = 0;
}

void Motor::SetSpeed(int percentage)
{
  this->desiredPercentage = constrain(percentage, -100, 100);
  this->desiredSpeed = (int)(this->desiredPercentage * (this->PWM_MAX/100.0));
}

int Motor::GetSpeed()
{
  return currentSpeed;
}

void Motor::Tick() {

  unsigned long deadline = this->changeTime + Motor::TICK_MS;
  unsigned long currentTime = millis();
  if (currentTime >= deadline) {
    
    
    if(this->currentSpeed >= this->desiredSpeed) {
      // need to run slower
      if(this->currentSpeed - this->SPEED_STEP >= this->desiredSpeed) {
        this->currentSpeed -= this->SPEED_STEP;
      }
      else {
        this->currentSpeed = this->desiredSpeed;
      }
    }
    else {
      // need to run faster
      if(this->currentSpeed + this->SPEED_STEP <= this->desiredSpeed) {
        this->currentSpeed += this->SPEED_STEP;
      }
      else {
        this->currentSpeed = this->desiredSpeed;
      }
    }
    unsigned long now = millis();
    this->changeTime = now;
    //Log.Debug("[%l] Motor tick: desired [%d]%% (%d),  current [%d]"CR, now, this->desiredPercentage, this->desiredSpeed, this->currentSpeed);
    this->PortIO();
  }
}

void Motor::PortIO() {
  analogWrite(this->PWM, abs(this->currentSpeed));
  
  int a = this->currentSpeed > 0 ? HIGH : LOW;
  int b = this->currentSpeed >= 0 ? LOW : HIGH;
  // will result in (a, b) = (LOW, LOW) if value == 0
  
  digitalWrite(this->FWD, a);
  digitalWrite(this->BCK, b);
}


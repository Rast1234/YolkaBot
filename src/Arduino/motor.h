#ifndef motor_h
#define motor_h

class Motor
{
  public:
    Motor(byte forward, byte back, byte pwm);

    void Stop();
    void Brake();

    void SetSpeed(int percentage);
    int GetSpeed();
    void Tick();

  private:
    byte PWM, FWD, BCK;  // configured pins

    int currentSpeed;  // immediate speed
    int desiredSpeed;  // stored speed
    int desiredPercentage;  // stored percentage
    unsigned long changeTime;  // last changed speed millisecond

    void PortIO();

    static int const PWM_MAX;      // i am trying not to kill 6V-motors with 12V-battery
                                   // so will use half of maximum possible value (255/2)
    
    static int const SPEED_STEP;   // will change PWM for this value per tick
    
    static int const TICK_MS;      // change speed every TICK_MS milliseconds
};

#endif

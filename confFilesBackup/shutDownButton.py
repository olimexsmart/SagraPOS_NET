import time
import RPi.GPIO as GPIO

# Usable only on RaspberryPi
# Launch this script adding a line in:
# sudo nano /etc/rc.local
# ex: python /home/pi/safe_shutdown_Pi.py &

# Pin definition
shutdownPin = 2
redPin = 3
greenPin = 4
# Suppress warnings
GPIO.setwarnings(False)
# Use "GPIO" pin numbering
GPIO.setmode(GPIO.BCM)
# Use built-in internal pullup resistor so the pin is not floating
# if using a momentary push button without a resistor.
GPIO.setup(shutdownPin, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(greenPin, GPIO.OUT)
GPIO.setup(redPin, GPIO.OUT)

while True:
    time.sleep(1)
    counter = 0
    # print('GPIO state is = ', GPIO.input(shutdownPin))
    GPIO.output(redPin, False)
    GPIO.output(greenPin, True)

    while GPIO.input(shutdownPin) == False:
        # Intermittent red LED
        GPIO.output(redPin, counter % 2 == 0)
        GPIO.output(greenPin, False)
        print(counter)
        counter += 1
        # Confirm shutdown after 3 seconds
        if counter > 30:
            GPIO.output(redPin, True)
            # print("shutting down")
            command = "/usr/bin/sudo /sbin/shutdown -h now"
            import subprocess
            process = subprocess.Popen(command.split(), stdout=subprocess.PIPE)
            output = process.communicate()[0]
            # print(output)

        time.sleep(0.1)

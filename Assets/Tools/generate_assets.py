"""Genera sprites pixel-art y sonidos para el juego (ya incluidos en Assets/). Uso: python3 Tools/generate_assets.py"""
import math, random, wave, struct, os
from PIL import Image, ImageDraw

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Assets")
SP = os.path.join(ROOT, "Sprites"); AU = os.path.join(ROOT, "Resources", "Audio")
os.makedirs(SP, exist_ok=True); os.makedirs(AU, exist_ok=True)
random.seed(7)

def save(im, name): im.save(os.path.join(SP, name + ".png"))

def soldier(name, pal, legs, bob=0, gun_dx=0, flash=False):
    helmet, skin, vest, pants, boots, gun = pal
    im = Image.new("RGBA", (24, 32), (0, 0, 0, 0)); d = ImageDraw.Draw(im); b = bob
    for lx, lift in legs:
        d.rectangle([lx, 21 + b, lx + 3, 29 - lift], fill=pants)
        d.rectangle([lx, 29 - lift, lx + 4, 31 - lift], fill=boots)
    d.rectangle([7, 12 + b, 16, 21 + b], fill=vest)
    d.rectangle([7, 20 + b, 16, 21 + b], fill=(110, 70, 30))
    d.rectangle([8, 6 + b, 15, 12 + b], fill=skin)
    d.rectangle([7, 2 + b, 16, 7 + b], fill=helmet)
    d.rectangle([14, 6 + b, 17, 7 + b], fill=helmet)
    d.point((14, 9 + b), fill=(20, 20, 20))
    d.rectangle([13, 13 + b, 17, 15 + b], fill=skin)
    d.rectangle([15 + gun_dx, 13 + b, 21 + gun_dx, 15 + b], fill=gun)
    d.rectangle([15 + gun_dx, 16 + b, 17 + gun_dx, 17 + b], fill=gun)
    if flash:
        d.rectangle([22, 12 + b, 23, 16 + b], fill=(255, 230, 80))
    save(im, name)

P = ((60, 110, 60), (230, 180, 140), (40, 80, 160), (60, 60, 100), (90, 55, 30), (50, 50, 55))
E = ((140, 40, 40), (225, 175, 135), (120, 100, 60), (90, 50, 50), (25, 25, 25), (40, 40, 40))
idle = [(8, 0), (12, 0)]
soldier("player_idle0", P, idle); soldier("player_idle1", P, idle, bob=1)
soldier("player_walk0", P, [(6, 0), (13, 0)]); soldier("player_walk1", P, [(8, 2), (12, 0)], bob=1)
soldier("player_walk2", P, [(9, 0), (10, 0)]); soldier("player_walk3", P, [(8, 0), (12, 2)], bob=1)
soldier("player_shoot0", P, idle, flash=True); soldier("player_shoot1", P, idle, gun_dx=-1)
soldier("player_jump0", P, [(8, 3), (12, 1)])
soldier("enemy_walk0", E, [(6, 0), (13, 0)]); soldier("enemy_walk1", E, [(9, 0), (10, 0)], bob=1)

def tank(name, tread, flash):
    im = Image.new("RGBA", (64, 40), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.rounded_rectangle([2, 27, 61, 39], 5, fill=(40, 40, 45))
    for i in range(6 + tread, 60, 8): d.ellipse([i - 3, 29, i + 3, 37], fill=(90, 90, 95))
    d.rectangle([6, 15, 57, 28], fill=(80, 100, 70)); d.rectangle([6, 15, 57, 17], fill=(110, 135, 95))
    d.rectangle([18, 5, 43, 16], fill=(95, 120, 82)); d.rectangle([22, 8, 28, 11], fill=(30, 30, 30))
    d.rectangle([43, 9, 62, 12], fill=(45, 45, 50))
    d.rectangle([10, 20, 14, 23], fill=(150, 40, 40))
    if flash: d.polygon([(62, 7), (63, 10), (62, 14)], fill=(255, 230, 80))
    save(im, name)
tank("boss0", 0, False); tank("boss1", 4, False); tank("boss_shoot", 0, True)

im = Image.new("RGBA", (8, 3), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 7, 2], fill=(255, 210, 40)); d.rectangle([1, 1, 6, 1], fill=(255, 255, 220)); save(im, "bullet")
im = Image.new("RGBA", (6, 6), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
d.ellipse([0, 0, 5, 5], fill=(255, 100, 30)); d.ellipse([2, 2, 3, 3], fill=(255, 240, 150)); save(im, "enemy_bullet")

for i, w in enumerate([5, 4, 2, 4]):
    im = Image.new("RGBA", (12, 12), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.ellipse([6 - w, 1, 6 + w, 10], fill=(255, 205, 40), outline=(190, 130, 10))
    if w > 3: d.rectangle([5, 4, 6, 7], fill=(255, 240, 150))
    save(im, f"coin{i}")
im = Image.new("RGBA", (12, 12), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 11, 11], fill=(245, 245, 245), outline=(120, 120, 120))
d.rectangle([5, 2, 6, 9], fill=(220, 30, 40)); d.rectangle([2, 5, 9, 6], fill=(220, 30, 40)); save(im, "health")

for i, (r, c) in enumerate([(6, (255, 220, 80)), (11, (255, 140, 30)), (14, (230, 80, 20)), (15, (90, 80, 80))]):
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.ellipse([16 - r, 16 - r, 16 + r, 16 + r], fill=c)
    if i in (1, 2): d.ellipse([16 - r // 2, 16 - r // 2, 16 + r // 2, 16 + r // 2], fill=(255, 230, 120))
    save(im, f"explosion{i}")

im = Image.new("RGBA", (16, 16), (110, 75, 40, 255)); px = im.load()
for _ in range(40): px[random.randrange(16), random.randrange(16)] = (85, 55, 28, 255)
for _ in range(14): px[random.randrange(16), random.randrange(16)] = (140, 100, 60, 255)
save(im, "dirt")
im = Image.new("RGBA", (16, 6), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 15, 2], fill=(80, 170, 70)); d.rectangle([0, 3, 15, 5], fill=(110, 75, 40))
for x in range(0, 16, 3): d.point((x, 3), fill=(80, 170, 70))
d.line([0, 0, 15, 0], fill=(130, 210, 90)); save(im, "grass")
im = Image.new("RGBA", (16, 8), (150, 150, 155, 255)); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 15, 7], outline=(80, 80, 90)); d.line([8, 0, 8, 7], fill=(100, 100, 110)); d.line([0, 4, 15, 4], fill=(100, 100, 110)); save(im, "platform")

# fondos (tileables en horizontal)
im = Image.new("RGBA", (256, 64), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
for cx, cy in [(30, 40), (95, 25), (160, 45), (220, 30)]:
    for k in (-256, 0, 256):
        for dx, dy, rx, ry in [(0, 0, 22, 9), (-14, 4, 14, 7), (15, 4, 16, 7)]:
            d.ellipse([cx + dx + k - rx, cy + dy - ry, cx + dx + k + rx, cy + dy + ry], fill=(255, 255, 255, 200))
save(im, "clouds")
im = Image.new("RGBA", (256, 96), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
for x in range(256):
    h = 40 + 18 * math.sin(2 * math.pi * x / 256) + 9 * math.sin(2 * math.pi * 3 * x / 256 + 1)
    d.line([x, 96 - int(h), x, 95], fill=(120, 160, 175, 255))
save(im, "hills_far")
im = Image.new("RGBA", (256, 96), (0, 0, 0, 0)); d = ImageDraw.Draw(im); x = 0
while x < 240:
    w = random.randint(14, 30); h = random.randint(30, 78)
    d.rectangle([x, 96 - h, min(x + w, 255), 95], fill=(70, 92, 105, 255))
    for wy in range(96 - h + 5, 90, 9):
        for wx in range(x + 3, min(x + w, 255) - 3, 7): d.rectangle([wx, wy, wx + 2, wy + 3], fill=(40, 55, 65, 255))
    if random.random() < 0.5: d.polygon([(x, 96 - h), (x + 6, 96 - h - 6), (x + 12, 96 - h)], fill=(70, 92, 105, 255))
    x += w + random.randint(2, 8)
save(im, "ruins_near")

# ---------- audio ----------
SR = 22050
def wav(name, samples):
    with wave.open(os.path.join(AU, name + ".wav"), "w") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s)) * 30000)) for s in samples))
def tone(f0, f1, dur, vol=0.5, shape="sq"):
    n = int(SR * dur); out = []; ph = 0
    for i in range(n):
        f = f0 + (f1 - f0) * i / n; ph += f / SR; env = 1 - i / n
        v = (1 if ph % 1 < 0.5 else -1) if shape == "sq" else (2 * (ph % 1) - 1 if shape == "saw" else math.sin(2 * math.pi * ph))
        out.append(v * vol * env)
    return out
def noise(dur, vol=0.5, power=1.5):
    n = int(SR * dur); return [(random.random() * 2 - 1) * vol * (1 - i / n) ** power for i in range(n)]
def mix(a, b): 
    n = max(len(a), len(b)); a = a + [0] * (n - len(a)); b = b + [0] * (n - len(b)); return [x + y for x, y in zip(a, b)]
wav("shoot", mix(noise(0.12, 0.4), tone(900, 200, 0.12, 0.3)))
wav("jump", tone(300, 700, 0.18, 0.35))
wav("coin", tone(988, 988, 0.06, 0.3) + tone(1319, 1319, 0.16, 0.3))
wav("hit", noise(0.1, 0.5))
wav("hurt", tone(260, 90, 0.3, 0.45, "saw"))
wav("explosion", mix(noise(0.55, 0.9, 2), tone(120, 40, 0.5, 0.4, "saw")))
wav("pickup", sum([tone(f, f, 0.07, 0.3) for f in (523, 659, 784, 1047)], []))
wav("win", sum([tone(f, f, 0.18, 0.35) for f in (523, 659, 784, 1047, 784, 1047)], []))
wav("gameover", sum([tone(f, f, 0.25, 0.35, "saw") for f in (392, 330, 262, 196)], []))
def music(bpm, bass, lead):
    beat = 60 / bpm / 2; out = []
    for i in range(16):
        b = tone(bass[i % len(bass)], bass[i % len(bass)], beat, 0.25, "tri")
        l = tone(lead[i % len(lead)], lead[i % len(lead)], beat, 0.14, "sq")
        out += mix(b, l)
    return out
wav("music_menu", music(100, [110, 110, 147, 131], [440, 523, 659, 523, 587, 523, 440, 392]))
wav("music_level1", music(140, [110, 110, 165, 147], [659, 784, 880, 784, 659, 587, 523, 587]))
wav("music_level2", music(125, [98, 98, 131, 117], [392, 466, 523, 466, 392, 349, 311, 349]))
print("ok")

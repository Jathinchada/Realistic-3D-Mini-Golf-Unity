import sys
from PIL import Image, ImageDraw, ImageFont

def generate_map():
    w, h = 2048, 1536
    img = Image.new("RGBA", (w, h), (42, 85, 48, 255)) # Rough grass background
    draw = ImageDraw.Draw(img)

    # Outer perimeter boundary wall (dashed / rounded)
    margin = 70
    draw.rounded_rectangle([margin, margin, w - margin, h - margin], radius=100, outline=(25, 25, 25, 255), width=12)

    # Pathways (light grey winding routes)
    path_color = (175, 172, 168, 255)
    # Main path from entrance to pavilion and across
    draw.line([(w - 200, h - 180), (w - 750, h - 220), (w - 1100, h - 550), (w - 1450, h - 850), (450, 400)], fill=path_color, width=44)
    draw.line([(w - 1100, h - 550), (w - 650, 450), (w - 900, 300)], fill=path_color, width=38)
    draw.line([(w - 1450, h - 850), (450, h - 450)], fill=path_color, width=38)

    # Upper Pond & Lower Pond
    water_color = (48, 138, 185, 255)
    water_border = (80, 85, 80, 255)
    # Upper East Pond
    draw.ellipse([w//2 + 50, 280, w//2 + 520, 680], fill=water_color, outline=water_border, width=12)
    # Lower West Pond
    draw.ellipse([w//2 - 420, h//2 + 50, w//2 + 50, h//2 + 480], fill=water_color, outline=water_border, width=12)

    # Streams connecting to ponds
    draw.line([(320, 480), (w//2 + 80, 480)], fill=water_color, width=50)
    draw.line([(240, h//2 + 150), (w//2 - 380, h//2 + 250)], fill=water_color, width=44)

    # Bridges
    bridge_color = (130, 85, 45, 255)
    draw.rectangle([w//2 + 180, 440, w//2 + 270, 520], fill=bridge_color, outline=(60, 35, 15, 255), width=4)
    draw.rectangle([w//2 - 220, h//2 + 180, w//2 - 130, h//2 + 260], fill=bridge_color, outline=(60, 35, 15, 255), width=4)

    # Pavilion 15x25 (near bottom-center)
    pav_x, pav_y = w//2 - 120, h - 260
    draw.rectangle([pav_x, pav_y, pav_x + 240, pav_y + 140], fill=(160, 110, 65, 255), outline=(50, 30, 10, 255), width=6)
    draw.text((pav_x + 35, pav_y + 55), "PAVILION\n 15' x 25'", fill=(255, 255, 255, 255))

    # Practice Putting Green (lower-right near entrance)
    pg_x, pg_y = w - 480, h - 340
    draw.rounded_rectangle([pg_x, pg_y, pg_x + 220, pg_y + 180], radius=25, fill=(38, 175, 75, 255), outline=(180, 50, 35, 255), width=6)
    draw.text((pg_x + 25, pg_y + 75), "PRACTICE GREEN", fill=(255, 255, 255, 255))

    # Holes definition (approx coordinates matching the blueprint anchors)
    holes = [
        # (num, name, x, y, width, height, par)
        (1, "Hole 1", w - 340, h - 520, 110, 240, 2),
        (2, "Hole 2", w - 240, h//2 - 100, 100, 380, 2),
        (3, "Hole 3", w - 420, 220, 260, 120, 2),
        (4, "Hole 4", w - 740, 320, 110, 260, 2),
        (5, "Hole 5", w - 480, h//2 - 120, 110, 220, 2),
        (6, "Hole 6", w - 680, h//2 + 80, 190, 110, 2),
        (7, "Hole 7", w//2 - 250, 180, 380, 110, 3),
        (8, "Hole 8", 420, 190, 320, 110, 2),
        (9, "Hole 9", 190, 260, 160, 220, 3),
        (10, "Hole 10", 170, 560, 110, 320, 2),
        (11, "Hole 11", 420, 440, 160, 260, 3),
        (12, "Hole 12", 480, h//2 + 100, 120, 260, 2),
        (13, "Hole 13", 260, h - 420, 200, 160, 2),
        (14, "Hole 14", 580, h - 400, 260, 110, 2),
        (15, "Hole 15", w//2 - 180, 480, 110, 240, 2),
        (16, "Hole 16", w//2 - 80, h//2 - 80, 110, 260, 3),
        (17, "Hole 17", w//2 + 60, h - 380, 260, 110, 2),
        (18, "Hole 18", w//2 + 360, h - 420, 240, 140, 3)
    ]

    green_color = (35, 178, 72, 255)
    rail_color = (185, 55, 42, 255)

    for num, name, x, y, width, height, par in holes:
        draw.rounded_rectangle([x, y, x + width, y + height], radius=20, fill=green_color, outline=rail_color, width=6)
        # Hole cup circle
        cup_x = x + width - 35 if width > height else x + width//2
        cup_y = y + 35 if width > height else y + height - 35
        draw.ellipse([cup_x - 12, cup_y - 12, cup_x + 12, cup_y + 12], fill=(20, 20, 20, 255), outline=(255, 255, 255, 255), width=2)
        # Tee marker circle
        tee_x = x + 35 if width > height else x + width//2
        tee_y = y + height//2 if width > height else y + 35
        draw.rectangle([tee_x - 10, tee_y - 10, tee_x + 10, tee_y + 10], fill=(240, 240, 240, 255))
        # Label
        draw.text((x + 15, y + 15), f"#{num} (P{par})", fill=(255, 255, 255, 255))

    # Waterfalls
    draw.polygon([(300, 440), (330, 440), (315, 520)], fill=(220, 240, 255, 255))
    draw.polygon([(220, h//2 + 130), (250, h//2 + 130), (235, h//2 + 190)], fill=(220, 240, 255, 255))

    # Save image
    out_path = "/Users/sushruthchada/Downloads/sru/gmdd project/Assets/Textures/MiniGolf_ReferenceMap.png"
    img.save(out_path, "PNG")
    print("Generated reference blueprint texture:", out_path)

if __name__ == "__main__":
    generate_map()

from PIL import Image
import os

# 打开图片
img = Image.open('778cb548-ce49-4f01-8f05-58f9c6bba79d.png')

# 获取图片宽高
width, height = img.size
print(f'原始图片尺寸: {width}x{height}')

# 假设图片有3个图标横向排列
third_width = width // 3

# 拆分三个图标
for i in range(3):
    left = i * third_width
    upper = 0
    right = (i + 1) * third_width
    lower = height
    
    icon = img.crop((left, upper, right, lower))
    
    if i == 0:
        name = 'WorldPlayStart.png'
    elif i == 1:
        name = 'WorldPlayBackup.png'
    else:
        name = 'WorldPlayDelete.png'
    
    # 保存到目标目录
    target_path = os.path.join('Plain Craft Launcher 2', 'Images', 'Icons', name)
    icon.save(target_path)
    print(f'保存: {target_path}')

print('图片拆分完成！')
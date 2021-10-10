# import numpy as np
# import quaternion
import os
import yaml
import sys
import shutil
import math


def processFile(filePath):
    try:
        print("* Processing \"" + filePath + "\"")
        # check if file exists
        if (not os.path.isfile(filePath)):
            print("! Nonexistent file")
            raise FileNotFoundError

        # move original file to backup location
        originalFilePath = moveToBackup(filePath)

        # read the file
        with open(originalFilePath) as file:
            fileContents = yaml.full_load(file)
            print("  File read")

            # process the contents
            # print(fileContents['ScriptedImporter']['characterData']['bones'])
            # bones = fileContents['ScriptedImporter']['rigPSDLayers']['characterData']['bones']
            bones = fileContents['ScriptedImporter']['characterData']['bones']
            print(bones)
            for bone in bones:
                print("  new bone:")
                print(bone)
                maybeRotateAngle(bone['rotation'])

            sprites = fileContents['ScriptedImporter']['rigSpriteImportData']
            print(sprites)
            for sprite in sprites:
                for spriteBone in sprite['spriteBone']:
                    print("  new sprite bone:")
                    print(spriteBone)
                    maybeRotateAngle(spriteBone['rotation'])

        # re-write the yaml to the original file path
            with open(filePath, 'w') as file:
                yaml.dump(fileContents, file)
                print("  File written")

    # return true if success, false if failed
    except:
        print("! Failure")
        print("  ", sys.exc_info())
        return False
    else:
        print("> Success")
        return True


def moveToBackup(filePath):
    if (os.path.exists(filePath)):
        return filePath
    else:
        shutil.copy2(filePath, filePath + "_copy.meta")
        return filePath


def toAngleAxis(quat):
    res = AngleAxis()
    angleRad = 2 * math.acos(quat['w'])

    # skip if angle is 0 (leads to divide by zero errors too)
    if (angleRad == 0):
        return res

    res.angle = angleRad
    quot = math.sqrt(1 - math.pow(quat['w'], 2))

    # probably shouldn't continue here but idk i'm dumb and i don't
    # want to deal with div by zero exception rn
    if (quot == 0):
        return res

    res.x = quat['x'] / quot
    res.y = quat['y'] / quot
    res.z = quat['z'] / quot
    return res


def roundToClosest(angleAxis, delta):
    angle = angleAxis.angle
    if (angle < -2 * math.pi or angle > 2 * math.pi):
        angle %= (2 * math.pi)

    halfPi = math.pi / 2.0

    multOf90 = angle / halfPi
    floor = math.floor(multOf90)
    ceil = math.ceil(multOf90)
    closest = floor if abs(multOf90 - floor) < abs(multOf90 - ceil) else ceil
    closest *= halfPi

    return closest if abs(closest - angle) < delta else angle

    # floor = halfPi * (angle > 0 ? angle / halfPi : angle - halfPi / halfPi)
    # floor = angle / halfPi if angle > 0 else angle - halfPi / halfPi


class AngleAxis:
    angle = 0
    x = 0
    y = 0
    z = 0

    def __str__(self):
        return "angle: " + str(self.angle) + " x: " + str(self.x) + " y: " + str(self.y) + " z: " + str(self.z)


def maybeRotateAngle(rotation):
    angleAxis = toAngleAxis(rotation)
    print(angleAxis)
    angleAxis.angle = roundToClosest(angleAxis, 15 * math.pi / 180)
    print(angleAxis)
    sin = math.sin(angleAxis.angle / 2.0)
    rotation['x'] = round(angleAxis.x * sin, 8)
    rotation['y'] = round(angleAxis.y * sin, 8)
    rotation['z'] = round(angleAxis.z * sin, 8)
    rotation['w'] = round(math.cos(angleAxis.angle / 2.0), 8)


def main(argv):
    print(argv)
    if len(argv) != 1:
        print("! Invalid args, need file to convert")
        sys.exit("Invalid args")
    processFile(argv[0])


if __name__ == "__main__":
    main(sys.argv[1:])

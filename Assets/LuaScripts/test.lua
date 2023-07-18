-- Say("hello\ngood morning!\ngood afternoon!\ngood night!")
Say("fog","hello")

local result = YesNo()

if result == 0 then
    Say("mipo","高興")
else
    Say("agent","非常感謝")
end

-- SetSkipStart()

local fog = Character("fog", "")
Say(fog,"こんにちは！")

-- result = Question({"朝ごはん","昼ご飯","晩御飯"})

-- if result == 0 then
--     Say("mipo","好吃")
-- elseif result == 1 then
--     Say("mipo","普通")
-- else
-- end

Exit()


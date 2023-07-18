local fog = Character("fog", "")

Say(fog, "こんにちは\nおはよう")
Say("村人A", "てすと")
Say("", "てすと2")

local ret = YesNo()

if ret == 0 then
    Say("","はい")
else
    Say("","いいえ")
end

ret = Question({"afda","2","3"})

if ret == 0 then
    Say("","1")
elseif ret == 1 then
    Say("","2")
else
    Say("","3")
end

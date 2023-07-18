await = (require 'Assets/LuaScripts/Core/cs_coroutine.lua').yield_return
Wait = function (time)
    await(CS.UnityEngine.WaitForSeconds(time))
end

SetAnim = function(name, anim)
    
end

Character = function(name, anim)
    ShowCharacter()
    return {
        name = name,
        anim = anim,
    }
end

Say = function(person, msg)
    if type(person) == "string" then
        SetName(person)
    else
        SetName(person.name)
    end
    await(SayAsync(msg))
end

Question = function(select)
    KeepShow()
    await(QuestionAsync(select))
    return QuestionResult()
end

YesNo = function()    
    return Question({"是","否"})
end

Exit = function()
    CloseCharacter()
end


ShortMessage = function(msg)
    await(ShortMessageAsync(msg))
end

WebRequestGet = function(url)
    await(WebRequestGetAsync(url))
    return WebRequestGetResult()
end
import uvicorn
import json

from fastapi import FastAPI
from pydantic import BaseModel

from re_edge_gpt import Chatbot
from re_edge_gpt import ConversationStyle
from re_edge_gpt import ImageGenAsync

app = FastAPI()

class CreateBotReq(BaseModel):
    id: str
    cookies: str = None
    proxy: str = None

class BaseBotModel(BaseModel):
    id: str
    
conversationStyle = {}
conversationStyle["creative"] = ConversationStyle.creative
conversationStyle["balanced"] = ConversationStyle.balanced
conversationStyle["precise"] = ConversationStyle.precise
    
class AskBotReq(BaseModel):
    id: str
    prompt: str
    wss_link: str = None,
    conversation_style: str = None,
    webpage_context: str = None,
    search_result: bool = False,
    attachment: dict[str, str] = None

class AskBotResp(BaseModel):
    code: int
    id: str
    response: dict

class GenImageReq(BaseModel):
    auth_cookie: str
    prompt: str
    proxy: str = None

chatbot = {}

@app.post("/create")
async def create_bot(req: CreateBotReq):
    cookies = None
    if isinstance(req.cookies, str) and req.cookies != "":
        cookies = json.loads(req.cookies)
    proxy=req.proxy if isinstance(req.proxy, str) else None
    bot = await Chatbot.create(cookies=cookies, proxy=proxy)
    print(req.id)
    chatbot[req.id] = bot
    return BaseBotModel(id=req.id)

@app.post("/ask")
async def ask_bot(req: AskBotReq):
    bot = chatbot.get(req.id)
    if bot is None:
        return AskBotResp(code=-1, id=req.id, response={})
    response = await bot.ask(
        prompt=req.prompt,
        wss_link=req.wss_link if isinstance(req.wss_link, str) else None,
        conversation_style=conversationStyle.get(req.conversation_style),
        webpage_context=req.webpage_context if isinstance(req.webpage_context, str) else None,
        search_result=req.search_result,
        simplify_response=False,
        attachment = req.attachment,
    )
    return AskBotResp(code=0, id=req.id, response=response)

@app.post("/reset")
async def close_bot(req: BaseBotModel):
    await chatbot.get(req.id).reset()
    return req

def remove_elements(arr, target):
    for i in range(len(arr)-1, -1, -1): # 注意这里要从最后一个元素开始向前遍历
        if str(arr[i]).rfind(target) != -1:
            del arr[i]

@app.post("/generate_image")
async def generate_image(req: GenImageReq):
    proxy=req.proxy if isinstance(req.proxy, str) else None
    gen = ImageGenAsync(auth_cookie=req.auth_cookie, proxy=proxy)
    image_list = await gen.get_images(req.prompt)
    image_list = image_list if image_list is not None else []
    remove_elements(image_list, '.svg')
    return image_list


if __name__ == '__main__':
	uvicorn.run(app='EdgeGPT:app', host="127.0.0.1", port=8989, reload=True)
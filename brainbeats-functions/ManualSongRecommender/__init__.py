import sys
sys.path.append('.')
sys.path.append('..')

import logging
import os
import json
import azure.functions as func
import requests
import time

from commons.SongRecommender import recommend_beats

async def main(req: func.HttpRequest) -> func.HttpResponse:
    req_body = req.get_json()

    logging.info(f'Python HTTP trigger function processed a request; body: {req_body}')
    
    email = req_body.get('email')

    if email is None:
        return func.HttpResponse(body = f'Malformed request: specify email', status_code = 400)

    try:
        await recommend_beats(email)
    except Exception as e:
        return func.HttpResponse(body = f'{e}', status_code = 400)

    return func.HttpResponse(status_code = 200)
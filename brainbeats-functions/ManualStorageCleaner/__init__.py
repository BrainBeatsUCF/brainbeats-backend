import sys
sys.path.append('.')
sys.path.append('..')

import logging
import azure.functions as func

from commons.storage_cleaner import clean_samples

async def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        deleted_samples = await clean_samples()
        return func.HttpResponse(f'Samples deleted: {deleted_samples}')
    except e as Exception:
        return func.HttpResponse(e, status_code = 400)


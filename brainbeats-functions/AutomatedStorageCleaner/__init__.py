import sys
sys.path.append('.')
sys.path.append('..')

import datetime
import logging
import azure.functions as func

from commons.storage_cleaner import clean_samples

def main(mytimer: func.TimerRequest) -> None:
    utc_timestamp = datetime.datetime.utcnow().replace(
        tzinfo=datetime.timezone.utc).isoformat()

    if mytimer.past_due:
        logging.info(f'Automated Storage Cleaner is past due: {utc_timestamp}')

    logging.info(f'Automated Storage Cleaner ran at {utc_timestamp}')

    try:
        deleted_samples = await clean_samples()
        logging.info(f'Deleted Samples in Automated Storage Cleaner: {deleted_samples}')
    except Exception as e:
        logging.info(f'Error in Automated Storage Cleaner: {e}')

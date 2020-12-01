import sys
sys.path.append('.')
sys.path.append('..')

import os
import logging
import azure.functions as func
import datetime

DATABASE_URL = os.environ['database_url']
DATABASE_NAME = os.environ['database_name']
DATABASE_COLLECTION = os.environ['database_collection']
DATABASE_KEY = os.environ['database_key']
LIMIT = int(os.environ['likes_limit'])
K = int(os.environ['KNN_K'])

from commons.DatabaseConnection import DatabaseConnection
from commons.SongRecommender import recommend_beats

db_conn = DatabaseConnection(DATABASE_URL, DATABASE_NAME, DATABASE_COLLECTION, DATABASE_KEY)

async def main(mytimer: func.TimerRequest) -> None:
    utc_timestamp = datetime.datetime.utcnow().replace(
        tzinfo = datetime.timezone.utc).isoformat()

    if mytimer.past_due:
        logging.info(f'Automated Song Recommender is past due: {utc_timestamp}')

    logging.info(f'Automated Song Recommender ran at {utc_timestamp}')

    all_users = db_conn.execute_query(db_conn.query_map['get_all_users'])
    for user in all_users:
        email = user['id']

        try:
            await recommend_beats(email)
        except Exception as e:
            logging.info(f'Error in KNN: {e}')

        logging.info(f'{email} completed recommendations')
    logging.info('Automated Song Recommender finished')

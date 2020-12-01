import sys
sys.path.append('.')
sys.path.append('..')

import os
import time

DATABASE_URL = os.environ['database_url']
DATABASE_NAME = os.environ['database_name']
DATABASE_COLLECTION = os.environ['database_collection']
DATABASE_KEY = os.environ['database_key']
LIMIT = int(os.environ['likes_limit'])
K = int(os.environ['KNN_K'])

from commons.DatabaseConnection import DatabaseConnection
from commons.KNN import KNN

db_conn = DatabaseConnection(DATABASE_URL, DATABASE_NAME, DATABASE_COLLECTION, DATABASE_KEY)

async def recommend_beats(email: str):
    """
    Generates recommendations for a single user corresponding to the input email.
    """

    # Get owned beats
    owned_beats = db_conn.execute_query(db_conn.query_map['get_owned_beats'].format(email = email))

    # Filter away owned beats so owned beats aren't recommended back to the same user:
    # Add all owned beat ids into a set
    owned_ids = set()
    for beat in owned_beats:
        owned_ids.add(beat['id'])

    # Get all public beats
    all_beats = db_conn.execute_query(db_conn.query_map['get_all_public_beats'])
    all_beats_filtered = list()

    # Add all beats not owned by the user into all_beats_filtered
    for beat in all_beats:
        if beat['id'] not in owned_ids:
            all_beats_filtered.append(beat)

    # Get liked beats
    liked_beats = db_conn.execute_query(db_conn.query_map['get_liked_beats'].format(email = email, 
        limit = LIMIT))

    try:
        # Get the K Recc'ed Beats
        knn = KNN(K)
        recc_beats = knn.get_k_nearest_neighbors(liked_beats, all_beats_filtered)

        current_time = int(time.time())

        # Drop the current recommendations
        db_conn.execute_query(db_conn.query_map['delete_recommendations'].format(email = email))

        for beat in recc_beats:
            db_conn.execute_query(db_conn.query_map['recommend_beat'].format(email = email,
                current_time = current_time, beat_id = beat['id']))
    except:
        raise
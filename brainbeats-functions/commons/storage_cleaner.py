import sys
sys.path.append('.')
sys.path.append('..')

import json
import os
from azure.storage.blob import BlobServiceClient

from commons.DatabaseConnection import DatabaseConnection

STORAGE_CONNECTION_STRING = os.getenv('storage_connection_string')
blob_service_client = BlobServiceClient.from_connection_string(STORAGE_CONNECTION_STRING)

DATABASE_URL = os.environ['database_url']
DATABASE_NAME = os.environ['database_name']
DATABASE_COLLECTION = os.environ['database_collection']
DATABASE_KEY = os.environ['database_key']

db_conn = DatabaseConnection(DATABASE_URL, DATABASE_NAME, DATABASE_COLLECTION, DATABASE_KEY)

async def clean_samples() -> list:
    beats = db_conn.execute_query(db_conn.query_map['get_all_beats'])
    samples = db_conn.execute_query(db_conn.query_map['get_all_samples'])

    samples_used = set()

    # Add all Sample Ids from Beat Attributes
    for vertex in beats:
        attribute_samples = json.loads(vertex['properties']['attributes'][0]['value'])
        for sample in attribute_samples:
            samples_used.add(sample['sampleID'])

    # Add all Sample Ids from existing Samples
    for vertex in samples:
        samples_used.add(vertex['id'])

    # Get the Samples container
    container_client = blob_service_client.get_container_client('sample')

    deleted_samples = list()

    # For each file (blob)...
    for blob in container_client.list_blobs():
        # If this is an audio file (wav or mp3):
        # and is never referenced in a Beat or Sample Vertex, delete it
        file_name = blob.name
        sample_id = file_name.split('_')[0]

        if sample_id not in samples_used:
            try:
                blob_client = container_client.get_blob_client(file_name)
                blob_client.delete_blob()
                deleted_samples.append(file_name)
            except e as Exception:
                raise

    return deleted_samples
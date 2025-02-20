create table platform.object_scoped_recommendation_v2
(
    account_id                 text,
    analytics_id               text,
    object_class               text,
    object_id                  text,            -- ID of the object being recommended (e.g. product_id)
    category                   text,            -- Category of the recommendation (e.g. 'html_correction', 'seo_correction')
    recommendation_data        text,            -- JSON Object, can include from/to language, etc.
    metadata                   text,
    summary                    text,            -- Short summary (140 chars max) for display on cards

    card_picture_url           text,            -- URL for the picture to display on the card
    card_picture_alt_text      text,            -- Alt text for the picture
    card_title                 text,            -- Title for the card, e.g., 'Product Description Correction'
    card_subtitle              text,            -- Subtitle for the card, e.g., product name

    lookup                     map<text, text>, -- Lookup data for the object being recommended, e.g., {'product_id': '1234'}

    updated_at                 timestamp,
    state                      text,
    muted_until                timestamp,
    feedback                   text,
    severity                   float,

    edited_recommendation_data text,            -- copy of recommendation_data that has been edited by the user

    algorithm_version          text,            -- Version of the algorithm that generated the recommendation

    PRIMARY KEY ((account_id, analytics_id, object_class), object_id, category)
);

create custom index recommendationv2_lookup_idx on platform.object_scoped_recommendation_v2 (ENTRIES (lookup))
    using 'StorageAttachedIndex';

create custom index recommendationv2_account_idx on platform.object_scoped_recommendation_v2 (account_id)
    using 'StorageAttachedIndex';

create custom index recommendationv2_category_idx on platform.object_scoped_recommendation_v2 (category)
    using 'StorageAttachedIndex';
